using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Repositorios;

namespace ProjectUploader.Aplicacao.CasosDeUso.Transferencia;

/// <summary>
/// Classe record que agrupa o estado/progresso de um envio para resposta em tempo real.
/// </summary>
public record ProgressoUpload(
    Guid IdArquivo, 
    string NomeOriginal, 
    long TotalBytes, 
    long BytesEnviados, 
    Dominio.Enums.StatusArquivo Status);

public interface IServicoTransferenciaLocal
{
    Task<bool> RealizarUploadArquivoAsync(ArquivoTransferencia arquivo, string caminhoLocalAbsoluto, IProgress<ProgressoUpload> progress, CancellationToken cancellationToken);
}

public class IniciarUploadLoteUseCase
{
    private readonly IRepositorioArquivo _repositorioArquivo;
    private readonly IServicoTransferenciaLocal _servicoTransferencia;

    public IniciarUploadLoteUseCase(
        IRepositorioArquivo repositorioArquivo,
        IServicoTransferenciaLocal servicoTransferencia)
    {
        _repositorioArquivo = repositorioArquivo;
        _servicoTransferencia = servicoTransferencia;
    }

    /// <summary>
    /// Inicia o upload de uma lista de arquivos paralelamente com controle max de limite de threads (999).
    /// </summary>
    public async Task ExecutarAsync(
        Guid idUsuario, 
        IReadOnlyDictionary<string, ArquivoTransferencia> caminhosEArquivos, 
        int maxConcorrencia,
        IProgress<ProgressoUpload> progress,
        CancellationToken cancellationToken)
    {
        if (caminhosEArquivos.Count > 999)
            throw new ArgumentOutOfRangeException(nameof(caminhosEArquivos), "O limite máximo é de 999 arquivos simultâneos.");

        // Salvar todos como "Pendentes" no banco inicialmente.
        foreach (var kvp in caminhosEArquivos)
        {
            await _repositorioArquivo.AdicionarAsync(kvp.Value);
        }

        // Semaphore para controlar as threads concorrentes.
        using var semaphore = new SemaphoreSlim(maxConcorrencia);

        var tasks = new List<Task>();

        foreach (var kvp in caminhosEArquivos)
        {
            var caminhoAbsoluto = kvp.Key;
            var arquivoInfo = kvp.Value;

            tasks.Add(Task.Run(async () => 
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    arquivoInfo.IniciarTransferencia();
                    await _repositorioArquivo.AtualizarAsync(arquivoInfo);

                    bool sucesso = await _servicoTransferencia.RealizarUploadArquivoAsync(arquivoInfo, caminhoAbsoluto, progress, cancellationToken);
                    
                    if (sucesso)
                        arquivoInfo.MarcarComoConcluido($"/uploads/{idUsuario}/{arquivoInfo.NomeOriginal}");
                    else
                        arquivoInfo.RegistrarErro("Falha no envio via serviço de transferência.");
                    
                    await _repositorioArquivo.AtualizarAsync(arquivoInfo);
                }
                catch (Exception ex)
                {
                    arquivoInfo.RegistrarErro(ex.Message);
                    await _repositorioArquivo.AtualizarAsync(arquivoInfo);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}
