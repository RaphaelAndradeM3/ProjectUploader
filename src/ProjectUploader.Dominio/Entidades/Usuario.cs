using System;
using ProjectUploader.Dominio.Enums;

namespace ProjectUploader.Dominio.Entidades;

/// <summary>
/// Raiz do Agregado representando um Usuário do sistema.
/// </summary>
public class Usuario
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public string NomeUsuario { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string EmailExtra { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public string WhatsApp { get; private set; } = string.Empty;
    public string Telegram { get; private set; } = string.Empty;
    public string DicaRecuperacao { get; private set; } = string.Empty;
    public TipoPerfil Perfil { get; private set; }
    
    // Controle de concorrência otimista (EF Core)
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // Construtor vazio para ORM
    protected Usuario() { }

    public Usuario(
        Guid id, 
        string nomeCompleto, 
        string nomeUsuario, 
        string email, 
        string senhaHash, 
        TipoPerfil perfil)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        AtualizarDadosBasicos(nomeCompleto, nomeUsuario, email);
        SenhaHash = senhaHash;
        Perfil = perfil;
    }

    public void AtualizarDadosBasicos(string nomeCompleto, string nomeUsuario, string email)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto)) throw new ArgumentException("Nome completo não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(nomeUsuario)) throw new ArgumentException("Nome de usuário não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email não pode ser vazio.");

        if (nomeUsuario.Contains(' '))
            throw new ArgumentException("O nome de usuário não pode conter espaços.");

        NomeCompleto = nomeCompleto;
        NomeUsuario = nomeUsuario;
        Email = email;
    }

    public void AtualizarContatos(string telefone, string whatsapp, string telegram, string emailExtra)
    {
        Telefone = telefone ?? string.Empty;
        WhatsApp = whatsapp ?? string.Empty;
        Telegram = telegram ?? string.Empty;
        EmailExtra = emailExtra ?? string.Empty;
    }

    public void AlterarSenha(string novaSenhaHash, string dicaRecuperacao)
    {
        ArgumentNullException.ThrowIfNull(novaSenhaHash);
        SenhaHash = novaSenhaHash;
        DicaRecuperacao = dicaRecuperacao ?? string.Empty;
    }
}
