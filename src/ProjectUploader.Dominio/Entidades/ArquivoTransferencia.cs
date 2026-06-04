using System;
using ProjectUploader.Dominio.Enums;

namespace ProjectUploader.Dominio.Entidades;

/// <summary>
/// Entidade que representa um arquivo enviado ou recebido.
/// </summary>
public class ArquivoTransferencia
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string NomeOriginal { get; private set; } = string.Empty;
    public string CaminhoServidor { get; private set; } = string.Empty;
    public long TamanhoBytes { get; private set; }
    public string HashVerificacao { get; private set; } = string.Empty;
    public StatusArquivo Status { get; private set; }
    public DateTime DataRegistro { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public string MensagemErro { get; private set; } = string.Empty;

    // Construtor para ORM
    protected ArquivoTransferencia() { }

    public ArquivoTransferencia(Guid idUsuario, string nomeOriginal, long tamanhoBytes, string hashVerificacao)
    {
        Id = Guid.NewGuid();
        IdUsuario = idUsuario;
        NomeOriginal = nomeOriginal ?? throw new ArgumentNullException(nameof(nomeOriginal));
        TamanhoBytes = tamanhoBytes;
        HashVerificacao = hashVerificacao ?? string.Empty;
        Status = StatusArquivo.Pendente;
        DataRegistro = DateTime.UtcNow;
    }

    public void IniciarTransferencia()
    {
        Status = StatusArquivo.EmAndamento;
    }

    public void MarcarComoConcluido(string caminhoServidor)
    {
        Status = StatusArquivo.Concluido;
        CaminhoServidor = caminhoServidor ?? string.Empty;
        DataConclusao = DateTime.UtcNow;
    }

    public void RegistrarErro(string mensagemErro)
    {
        Status = StatusArquivo.Erro;
        MensagemErro = mensagemErro ?? "Erro desconhecido.";
        DataConclusao = DateTime.UtcNow;
    }
}
