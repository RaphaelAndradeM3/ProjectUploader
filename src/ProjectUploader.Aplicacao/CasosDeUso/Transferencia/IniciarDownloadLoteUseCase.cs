using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Repositorios;

namespace ProjectUploader.Aplicacao.CasosDeUso.Transferencia;

public interface IServicoDownloadLocal
{
    Task<bool> RealizarDownloadArquivoAsync(ArquivoTransferencia arquivo, string pastaDestinoLocal, IProgress<ProgressoUpload> progress, CancellationToken cancellationToken);
}

public class IniciarDownloadLoteUseCase
{
    private readonly IRepositorioArquivo _repositorioArquivo;
    private readonly IServicoDownloadLocal _servicoDownload;

    public IniciarDownloadLoteUseCase(
        IRepositorioArquivo repositorioArquivo,
        IServicoDownloadLocal servicoDownload)
    {
        _repositorioArquivo = repositorioArquivo;
        _servicoDownload = servicoDownload;
    }

    /// <summary>
    /// Inicia o download de uma lista de arquivos paralelamente com controle max de limite de threads.
    /// </summary>
    public async Task ExecutarAsync(
        IEnumerable<ArquivoTransferencia> arquivosParaBaixar, 
        string pastaDestinoLocal,
        int maxConcorrencia,
        IProgress<ProgressoUpload> progress,
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(maxConcorrencia);
        var tasks = new List<Task>();

        foreach (var arquivoInfo in arquivosParaBaixar)
        {
            tasks.Add(Task.Run(async () => 
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    // Status não muda no banco central durante download local se ele já estava Concluído (upload).
                    // Mas caso o sistema permita re-download, poderíamos gerar eventos de auditoria aqui.
                    
                    bool sucesso = await _servicoDownload.RealizarDownloadArquivoAsync(arquivoInfo, pastaDestinoLocal, progress, cancellationToken);
                    
                    if (!sucesso)
                    {
                        // Aqui idealmente nós logaríamos a falha do download no Event Viewer local ou num log local,
                        // já que a tabela de ArquivoTransferencia representa o estado do arquivo no servidor.
                    }
                }
                catch (Exception)
                {
                    // Lidar com exceção de download local.
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
