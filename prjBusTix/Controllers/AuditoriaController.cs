using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Security;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditoriaController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditoriaController> _logger;

    public AuditoriaController(AppDbContext context, ILogger<AuditoriaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtener registros de auditoría con filtros
    /// GET /api/auditoria
    /// </summary>
    [HttpGet]
    [ClRequirePermission(ClAppPermissions.AuditoriaView)]
    public async Task<ActionResult> GetAuditoria(
        [FromQuery] string? tabla = null,
        [FromQuery] string? registroId = null,
        [FromQuery] string? usuarioId = null,
        [FromQuery] string? accion = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.AuditoriaCambios
                .Include(a => a.Usuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tabla))
                query = query.Where(a => a.TablaAfectada == tabla);

            if (!string.IsNullOrEmpty(registroId))
                query = query.Where(a => a.RegistroID == registroId);

            if (!string.IsNullOrEmpty(usuarioId))
                query = query.Where(a => a.UsuarioID == usuarioId);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.TipoOperacion == accion);

            if (fechaDesde.HasValue)
                query = query.Where(a => a.FechaHoraCambio >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.FechaHoraCambio <= fechaHasta.Value);

            var total = await query.CountAsync();
            var registros = await query
                .OrderByDescending(a => a.FechaHoraCambio)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.AuditoriaID,
                    tabla = a.TablaAfectada,
                    registroId = a.RegistroID,
                    accion = a.TipoOperacion,
                    fechaAccion = a.FechaHoraCambio,
                    usuario = a.Usuario != null ? new
                    {
                        id = a.Usuario.Id,
                        nombre = a.Usuario.NombreCompleto,
                        email = a.Usuario.Email
                    } : null,
                    datosPrevios = a.ValoresAnteriores,
                    datosPosteriores = a.ValoresNuevos
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = registros,
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros de auditoría");
            return StatusCode(500, new { message = "Error al obtener auditoría", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener historial de un registro específico
    /// GET /api/auditoria/{tabla}/{registroId}
    /// </summary>
    [HttpGet("{tabla}/{registroId}")]
    [ClRequirePermission(ClAppPermissions.AuditoriaView)]
    public async Task<ActionResult> GetHistorialRegistro(string tabla, string registroId)
    {
        try
        {
            var historial = await _context.AuditoriaCambios
                .Include(a => a.Usuario)
                .Where(a => a.TablaAfectada == tabla && a.RegistroID == registroId)
                .OrderByDescending(a => a.FechaHoraCambio)
                .Select(a => new
                {
                    a.AuditoriaID,
                    accion = a.TipoOperacion,
                    fechaAccion = a.FechaHoraCambio,
                    usuario = a.Usuario != null ? new
                    {
                        id = a.Usuario.Id,
                        nombre = a.Usuario.NombreCompleto,
                        email = a.Usuario.Email
                    } : null,
                    datosPrevios = a.ValoresAnteriores,
                    datosPosteriores = a.ValoresNuevos
                })
                .ToListAsync();

            if (!historial.Any())
                return NotFound(new { message = "No se encontró historial para este registro" });

            return Ok(new
            {
                success = true,
                tabla,
                registroId,
                totalCambios = historial.Count,
                historial
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial del registro");
            return StatusCode(500, new { message = "Error al obtener historial", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener estadísticas de auditoría
    /// GET /api/auditoria/estadisticas
    /// </summary>
    [HttpGet("estadisticas")]
    [ClRequirePermission(ClAppPermissions.AuditoriaView)]
    public async Task<ActionResult> GetEstadisticas(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        try
        {
            var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
            var hasta = fechaHasta ?? DateTime.Now;

            var registros = await _context.AuditoriaCambios
                .Where(a => a.FechaHoraCambio >= desde && a.FechaHoraCambio <= hasta)
                .ToListAsync();

            var estadisticas = new
            {
                periodo = new { desde, hasta },
                totalRegistros = registros.Count,
                porAccion = registros
                    .GroupBy(a => a.TipoOperacion)
                    .Select(g => new { accion = g.Key, total = g.Count() })
                    .OrderByDescending(x => x.total)
                    .ToList(),
                porTabla = registros
                    .GroupBy(a => a.TablaAfectada)
                    .Select(g => new { tabla = g.Key, total = g.Count() })
                    .OrderByDescending(x => x.total)
                    .ToList(),
                porUsuario = await _context.AuditoriaCambios
                    .Include(a => a.Usuario)
                    .Where(a => a.FechaHoraCambio >= desde && a.FechaHoraCambio <= hasta)
                    .GroupBy(a => new { a.UsuarioID, a.Usuario!.NombreCompleto, a.Usuario.Email })
                    .Select(g => new
                    {
                        usuarioId = g.Key.UsuarioID,
                        nombreCompleto = g.Key.NombreCompleto,
                        email = g.Key.Email,
                        total = g.Count()
                    })
                    .OrderByDescending(x => x.total)
                    .Take(10)
                    .ToListAsync(),
                porDia = registros
                    .GroupBy(a => a.FechaHoraCambio.Date)
                    .Select(g => new { fecha = g.Key, total = g.Count() })
                    .OrderBy(x => x.fecha)
                    .ToList()
            };

            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de auditoría");
            return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
        }
    }
}
