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
}
