using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectUploader.Dominio.Repositorios;

namespace ProjectUploader.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")] // Apenas Admin pode ver logs
public class LogsController : ControllerBase
{
    private readonly IRepositorioLogEvento _repositorioLog;

    public LogsController(IRepositorioLogEvento repositorioLog)
    {
        _repositorioLog = repositorioLog;
    }

    [HttpGet]
    public async Task<IActionResult> ListarLogs()
    {
        var logs = await _repositorioLog.ObterTodosAsync();
        return Ok(logs);
    }
}
