using System;
using System.Threading.Tasks;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Excecoes;
using ProjectUploader.Dominio.Repositorios;
using ProjectUploader.Aplicacao.Interfaces;

namespace ProjectUploader.Aplicacao.CasosDeUso.Autenticacao;

public class CredenciaisInvalidasException : ProjectUploaderException
{
    public CredenciaisInvalidasException() : base("Nome de usuário ou senha incorretos.") { }
}

public class AutenticarUsuarioUseCase
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IServicoCriptografia _servicoCriptografia;
    private readonly IRepositorioLogEvento _repositorioLog;

    public AutenticarUsuarioUseCase(
        IRepositorioUsuario repositorioUsuario,
        IServicoCriptografia servicoCriptografia,
        IRepositorioLogEvento repositorioLog)
    {
        _repositorioUsuario = repositorioUsuario;
        _servicoCriptografia = servicoCriptografia;
        _repositorioLog = repositorioLog;
    }

    /// <summary>
    /// Autentica um usuário usando Nome de Usuário ou E-mail.
    /// </summary>
    public async Task<Usuario> ExecutarAsync(string login, string senha, string ipOrigem)
    {
        ArgumentException.ThrowIfNullOrEmpty(login);
        ArgumentException.ThrowIfNullOrEmpty(senha);

        // Tenta buscar por email ou nome de usuário
        var usuario = await _repositorioUsuario.ObterPorEmailAsync(login) 
                   ?? await _repositorioUsuario.ObterPorNomeUsuarioAsync(login);

        if (usuario == null || !_servicoCriptografia.ValidarSenha(senha, usuario.SenhaHash))
        {
            await _repositorioLog.AdicionarAsync(new LogEvento(
                "Aviso", 
                "Falha de Login", 
                $"Tentativa falha para o login: {login}", 
                null, 
                ipOrigem));
            
            throw new CredenciaisInvalidasException();
        }

        await _repositorioLog.AdicionarAsync(new LogEvento(
            "Info", 
            "Login Sucesso", 
            $"Usuário logado com sucesso.", 
            usuario.Id, 
            ipOrigem));

        return usuario;
    }
}
