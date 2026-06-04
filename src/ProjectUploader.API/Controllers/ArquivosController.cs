using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectUploader.Dominio.Entidades;
using ProjectUploader.Dominio.Repositorios;

namespace ProjectUploader.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige JWT
public class ArquivosController : ControllerBase
{
    private readonly IRepositorioArquivo _repositorioArquivo;
    private readonly IRepositorioLogEvento _repositorioLog;
    private readonly ILogger<ArquivosController> _logger;

    public ArquivosController(
        IRepositorioArquivo repositorioArquivo,
        IRepositorioLogEvento repositorioLog,
        ILogger<ArquivosController> logger)
    {
        _repositorioArquivo = repositorioArquivo;
        _repositorioLog = repositorioLog;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10L * 1024 * 1024 * 1024)] // 10GB limite por request
    [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string hashOriginal)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Arquivo não fornecido.");

        var idUsuarioLogadoStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idUsuarioLogadoStr, out Guid idUsuario))
            return Unauthorized();

        try
        {
            var ipOrigem = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";

            // Registrar no banco (Pendente)
            var arquivoInfo = new ArquivoTransferencia(idUsuario, file.FileName, file.Length, hashOriginal);
            await _repositorioArquivo.AdicionarAsync(arquivoInfo);

            arquivoInfo.IniciarTransferencia();
            await _repositorioArquivo.AtualizarAsync(arquivoInfo);

            var diretorioDestino = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", idUsuario.ToString());
            if (!Directory.Exists(diretorioDestino))
                Directory.CreateDirectory(diretorioDestino);

            var caminhoServidor = Path.Combine(diretorioDestino, file.FileName);

            using (var stream = new FileStream(caminhoServidor, FileMode.Create))
            {
                // Copia o arquivo em chunks mantendo a memória baixa
                await file.CopyToAsync(stream);
            }

            // TODO: Aqui a gente validaria o Hash do arquivo salvo com o hashOriginal.
            
            arquivoInfo.MarcarComoConcluido(caminhoServidor);
            await _repositorioArquivo.AtualizarAsync(arquivoInfo);

            await _repositorioLog.AdicionarAsync(new LogEvento("Info", "Upload Sucesso", $"Arquivo {file.FileName} salvo.", idUsuario, ipOrigem));

            return Ok(new { arquivoInfo.Id, arquivoInfo.Status, arquivoInfo.DataConclusao });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no upload do arquivo {FileName}", file.FileName);
            return StatusCode(500, "Erro interno no servidor ao processar o arquivo.");
        }
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var arquivoInfo = await _repositorioArquivo.ObterPorIdAsync(id);
        if (arquivoInfo == null || arquivoInfo.Status != Dominio.Enums.StatusArquivo.Concluido)
            return NotFound("Arquivo não encontrado ou incompleto.");

        if (!System.IO.File.Exists(arquivoInfo.CaminhoServidor))
            return NotFound("O arquivo físico não foi encontrado no servidor.");

        var stream = new FileStream(arquivoInfo.CaminhoServidor, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        
        var ipOrigem = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";
        var idUsuarioLogadoStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = Guid.TryParse(idUsuarioLogadoStr, out Guid idUsuario);

        await _repositorioLog.AdicionarAsync(new LogEvento("Info", "Download", $"Arquivo {arquivoInfo.NomeOriginal} baixado.", idUsuario, ipOrigem));

        return File(stream, "application/octet-stream", arquivoInfo.NomeOriginal);
    }

    [HttpGet]
    public async Task<IActionResult> ListarArquivos()
    {
        var idUsuarioLogadoStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idUsuarioLogadoStr, out Guid idUsuario))
            return Unauthorized();

        // Se for admin, lista todos. Se for comum, lista apenas os seus.
        var isAdmin = User.IsInRole("Administrador");

        var arquivos = isAdmin 
            ? await _repositorioArquivo.ObterTodosAsync()
            : await _repositorioArquivo.ObterPorUsuarioAsync(idUsuario);

        return Ok(arquivos);
    }
}
