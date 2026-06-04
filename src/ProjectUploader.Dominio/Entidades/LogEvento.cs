using System;

namespace ProjectUploader.Dominio.Entidades;

/// <summary>
/// Entidade imutável para registrar log de eventos do sistema (Auditoria).
/// </summary>
public class LogEvento
{
    public Guid Id { get; private set; }
    public DateTime DataHora { get; private set; }
    public string Nivel { get; private set; } = string.Empty;
    public string Operacao { get; private set; } = string.Empty;
    public string Detalhes { get; private set; } = string.Empty;
    public Guid? IdUsuarioLogado { get; private set; }
    public string IpOrigem { get; private set; } = string.Empty;

    // Para o ORM
    protected LogEvento() { }

    public LogEvento(string nivel, string operacao, string detalhes, Guid? idUsuarioLogado, string ipOrigem)
    {
        Id = Guid.NewGuid();
        DataHora = DateTime.UtcNow;
        Nivel = nivel ?? throw new ArgumentNullException(nameof(nivel));
        Operacao = operacao ?? throw new ArgumentNullException(nameof(operacao));
        Detalhes = detalhes ?? string.Empty;
        IdUsuarioLogado = idUsuarioLogado;
        IpOrigem = ipOrigem ?? string.Empty;
    }
}
