using System;
using System.Threading.Tasks;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Repositorios;
using ProjectUploader.Dominio.Excecoes;

namespace ProjectUploader.Aplicacao.CasosDeUso.Usuarios;

public class UsuarioJaExisteException : ProjectUploaderException
{
    public UsuarioJaExisteException(string mensagem) : base(mensagem) { }
}

public record DadosCadastroUsuario(
    string NomeCompleto,
    string NomeUsuario,
    string Email,
    string SenhaPlana,
    string Telefone,
    string WhatsApp,
    string Telegram,
    string EmailExtra,
    Dominio.Enums.TipoPerfil Perfil);

public class CadastrarUsuarioUseCase
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly Interfaces.IServicoCriptografia _criptografia;

    public CadastrarUsuarioUseCase(
        IRepositorioUsuario repositorioUsuario,
        Interfaces.IServicoCriptografia criptografia)
    {
        _repositorioUsuario = repositorioUsuario;
        _criptografia = criptografia;
    }

    public async Task<Usuario> ExecutarAsync(DadosCadastroUsuario dados)
    {
        ArgumentNullException.ThrowIfNull(dados);

        if (await _repositorioUsuario.ObterPorEmailAsync(dados.Email) != null)
            throw new UsuarioJaExisteException("E-mail já está em uso.");

        if (await _repositorioUsuario.ObterPorNomeUsuarioAsync(dados.NomeUsuario) != null)
            throw new UsuarioJaExisteException("Nome de usuário já está em uso.");

        string senhaHash = _criptografia.GerarHash(dados.SenhaPlana);

        var usuario = new Usuario(
            Guid.NewGuid(),
            dados.NomeCompleto,
            dados.NomeUsuario,
            dados.Email,
            senhaHash,
            dados.Perfil);

        usuario.AtualizarContatos(dados.Telefone, dados.WhatsApp, dados.Telegram, dados.EmailExtra);

        await _repositorioUsuario.AdicionarAsync(usuario);

        return usuario;
    }
}
