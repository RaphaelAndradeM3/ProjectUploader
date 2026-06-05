using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Repositorios;
using ProjectUploader.Aplicacao.Interfaces;
using System.Security.Claims;
using ProjectUploader.Dominio.Enums;

namespace ProjectUploader.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IServicoCriptografia _servicoCriptografia;
    private readonly IRepositorioLogEvento _repositorioLog;

    public UsuariosController(
        IRepositorioUsuario repositorioUsuario,
        IServicoCriptografia servicoCriptografia,
        IRepositorioLogEvento repositorioLog)
    {
        _repositorioUsuario = repositorioUsuario;
        _servicoCriptografia = servicoCriptografia;
        _repositorioLog = repositorioLog;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ListarTodos()
    {
        var usuarios = await _repositorioUsuario.ObterTodosAsync();
        return Ok(usuarios);
    }

    public record CriarUsuarioRequest(string NomeCompleto, string NomeUsuario, string Email, string EmailExtra, string Telefone, string WhatsApp, string Telegram, string Senha, string DicaRecuperacao, TipoPerfil Perfil);

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioRequest req)
    {
        if (await _repositorioUsuario.ObterPorEmailAsync(req.Email) != null) return BadRequest("E-mail já cadastrado.");
        if (await _repositorioUsuario.ObterPorNomeUsuarioAsync(req.NomeUsuario) != null) return BadRequest("Nome de usuário já cadastrado.");

        var hash = _servicoCriptografia.GerarHash(req.Senha);
        var novo = new Usuario(Guid.NewGuid(), req.NomeCompleto, req.NomeUsuario, req.Email, hash, req.Perfil);
        novo.AtualizarContatos(req.Telefone, req.WhatsApp, req.Telegram, req.EmailExtra);
        novo.AlterarSenha(hash, req.DicaRecuperacao);

        await _repositorioUsuario.AdicionarAsync(novo);
        await LogAction("Criar Usuário", $"Usuário {req.NomeUsuario} criado pelo Admin.");

        return CreatedAtAction(nameof(ListarTodos), new { id = novo.Id }, novo);
    }

    public record AtualizarPerfilRequest(string NomeCompleto, string NomeUsuario, string Email, string Telefone, string WhatsApp, string Telegram, string EmailExtra);

    [HttpPut("perfil")]
    public async Task<IActionResult> AtualizarPerfil([FromBody] AtualizarPerfilRequest req)
    {
        var idLogado = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuario = await _repositorioUsuario.ObterPorIdAsync(idLogado);
        if (usuario == null) return NotFound();

        usuario.AtualizarDadosBasicos(req.NomeCompleto, req.NomeUsuario, req.Email);
        usuario.AtualizarContatos(req.Telefone, req.WhatsApp, req.Telegram, req.EmailExtra);

        await _repositorioUsuario.AtualizarAsync(usuario);
        await LogAction("Atualizar Perfil", $"Usuário {req.NomeUsuario} atualizou os próprios dados.");

        return Ok(usuario);
    }

    public record TrocarSenhaRequest(string SenhaAtual, string NovaSenha, string DicaRecuperacao);

    [HttpPut("senha")]
    public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaRequest req)
    {
        var idLogado = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuario = await _repositorioUsuario.ObterPorIdAsync(idLogado);
        if (usuario == null) return NotFound();

        if (!_servicoCriptografia.ValidarSenha(req.SenhaAtual, usuario.SenhaHash))
            return BadRequest("Senha atual incorreta.");

        var hash = _servicoCriptografia.GerarHash(req.NovaSenha);
        usuario.AlterarSenha(hash, req.DicaRecuperacao);

        await _repositorioUsuario.AtualizarAsync(usuario);
        await LogAction("Troca de Senha", $"Usuário {usuario.NomeUsuario} trocou a própria senha.");

        return Ok();
    }

    // Apenas Admin pode resetar a senha de QUALQUER usuário
    public record AdminTrocarSenhaRequest(Guid IdUsuario, string NovaSenha, string DicaRecuperacao);

    [HttpPut("admin/senha")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AdminTrocarSenha([FromBody] AdminTrocarSenhaRequest req)
    {
        var usuario = await _repositorioUsuario.ObterPorIdAsync(req.IdUsuario);
        if (usuario == null) return NotFound();

        var hash = _servicoCriptografia.GerarHash(req.NovaSenha);
        usuario.AlterarSenha(hash, req.DicaRecuperacao);

        await _repositorioUsuario.AtualizarAsync(usuario);
        await LogAction("Admin Reset Senha", $"Admin resetou a senha de {usuario.NomeUsuario}.");

        return Ok();
    }

    private async Task LogAction(string operacao, string detalhes)
    {
        var idLogadoStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = Guid.TryParse(idLogadoStr, out Guid idLogado);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Local";
        
        await _repositorioLog.AdicionarAsync(new LogEvento("Info", operacao, detalhes, idLogado, ip));
    }
}
