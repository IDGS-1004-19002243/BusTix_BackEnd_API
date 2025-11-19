using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Boletos;
using prjBusTix.Model;
using prjBusTix.Services;
using System.Security.Claims;

namespace prjBusTix.Controllers;

/// <summary>
/// Controlador para sincronización de validaciones offline
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Manager")]
public class SincronizacionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SincronizacionController> _logger;
    private readonly IValidacionService _validacionService;

    public SincronizacionController(AppDbContext context, ILogger<SincronizacionController> logger, IValidacionService validacionService)
    {
        _context = context;
        _logger = logger;
        _validacionService = validacionService;
    }

    /// <summary>
    /// Sincronizar validaciones offline en batch
    /// POST /api/sincronizacion/validaciones
    /// </summary>
    [HttpPost("validaciones")]
    public async Task<ActionResult<SincronizacionResponseDto>> SincronizarValidaciones(
        [FromBody] List<ValidacionSyncDto> validaciones)
    {
        if (validaciones == null || !validaciones.Any())
        {
            return BadRequest(new SincronizacionResponseDto
            {
                Success = false,
                Message = "No se enviaron validaciones para sincronizar",
                TotalRecibidas = 0
            });
        }

        var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _validacionService.ProcesarValidacionesAsync(validaciones, staffId);

        return Ok(result);
    }
}

