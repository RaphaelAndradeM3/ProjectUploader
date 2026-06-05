using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectUploader.API.Servicos;
using ProjectUploader.Aplicacao.CasosDeUso.Autenticacao;

namespace ProjectUploader.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AutenticarUsuarioUseCase _autenticarUsuarioUseCase;
    private readonly IServicoToken _servicoToken;

    public AuthController(
        AutenticarUsuarioUseCase autenticarUsuarioUseCase,
        IServicoToken servicoToken)
    {
        _autenticarUsuarioUseCase = autenticarUsuarioUseCase;
        _servicoToken = servicoToken;
    }

    public record LoginRequest(string Login, string Senha);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipOrigem = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";
            var usuario = await _autenticarUsuarioUseCase.ExecutarAsync(request.Login, request.Senha, ipOrigem);
            
            var token = _servicoToken.GerarToken(usuario);
            
            return Ok(new 
            {
                Token = token,
                Usuario = new { usuario.Id, usuario.NomeUsuario, usuario.Email, Perfil = usuario.Perfil.ToString() }
            });
        }
        catch (CredenciaisInvalidasException ex)
        {
            return Unauthorized(new { Erro = ex.Message });
        }
    }

    public record EsqueciSenhaRequest(string Email);

    [HttpPost("esqueci-senha")]
    public async Task<IActionResult> EsqueciSenha(
        [FromBody] EsqueciSenhaRequest request,
        [FromServices] ProjectUploader.Dominio.Repositorios.IRepositorioUsuario repositorioUsuario,
        [FromServices] ProjectUploader.Dominio.Repositorios.IRepositorioLogEvento repositorioLog)
    {
        var usuario = await repositorioUsuario.ObterPorEmailAsync(request.Email);
        if (usuario == null)
        {
            // Retornamos OK de propósito para não vazar se o e-mail existe
            return Ok(new { Mensagem = "Se o e-mail existir, um link de recuperação foi enviado." });
        }

        // Gerar token simples
        var token = System.Guid.NewGuid().ToString("N");
        usuario.GerarTokenRecuperacao(token, System.TimeSpan.FromHours(2));
        
        await repositorioUsuario.AtualizarAsync(usuario);

        var ipOrigem = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";
        await repositorioLog.AdicionarAsync(new ProjectUploader.Dominio.Entidades.LogEvento(
            "Info", "Recuperação Senha", $"Token gerado para {request.Email}. (Simulação de Envio: {token})", usuario.Id, ipOrigem));

        // Aqui seria o envio real de e-mail. Como não temos SMTP, logamos e retornamos na API apenas para debug/teste.
        return Ok(new { Mensagem = "Se o e-mail existir, um link de recuperação foi enviado.", DebugToken = token });
    }

    public record RedefinirSenhaRequest(string Email, string Token, string NovaSenha, string DicaRecuperacao);

    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha(
        [FromBody] RedefinirSenhaRequest request,
        [FromServices] ProjectUploader.Dominio.Repositorios.IRepositorioUsuario repositorioUsuario,
        [FromServices] ProjectUploader.Aplicacao.Interfaces.IServicoCriptografia servicoCriptografia,
        [FromServices] ProjectUploader.Dominio.Repositorios.IRepositorioLogEvento repositorioLog)
    {
        var usuario = await repositorioUsuario.ObterPorEmailAsync(request.Email);
        if (usuario == null) return BadRequest("E-mail ou token inválido.");

        if (!usuario.ValidarEUsarTokenRecuperacao(request.Token))
            return BadRequest("Token inválido ou expirado.");

        var senhaHash = servicoCriptografia.GerarHash(request.NovaSenha);
        usuario.AlterarSenha(senhaHash, request.DicaRecuperacao);

        await repositorioUsuario.AtualizarAsync(usuario);

        var ipOrigem = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";
        await repositorioLog.AdicionarAsync(new ProjectUploader.Dominio.Entidades.LogEvento(
            "Info", "Senha Alterada", $"Senha alterada via recuperação.", usuario.Id, ipOrigem));

        return Ok(new { Mensagem = "Senha redefinida com sucesso." });
    }
}
