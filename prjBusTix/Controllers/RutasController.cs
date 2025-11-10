using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Rutas;
using prjBusTix.Model;
using prjBusTix.Security;
using System.Security.Claims;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RutasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<RutasController> _logger;

    public RutasController(AppDbContext context, ILogger<RutasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todas las plantillas de rutas
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PlantillaRutaResponseDto>>> GetRutas(
        [FromQuery] bool? soloActivas = null)
    {
        try
        {
            var query = _context.PlantillasRutas
                .Include(r => r.Paradas)
                .Include(r => r.Viajes)
                .AsQueryable();

            if (soloActivas.HasValue)
                query = query.Where(r => r.Activa == soloActivas.Value);

            var rutas = await query
                .OrderBy(r => r.NombreRuta)
                .Select(r => new PlantillaRutaResponseDto
                {
                    RutaID = r.RutaID,
                    CodigoRuta = r.CodigoRuta,
                    NombreRuta = r.NombreRuta,
                    CiudadOrigen = r.CiudadOrigen,
                    CiudadDestino = r.CiudadDestino,
                    PuntoPartidaLat = r.PuntoPartidaLat,
                    PuntoPartidaLong = r.PuntoPartidaLong,
                    PuntoPartidaNombre = r.PuntoPartidaNombre,
                    PuntoLlegadaLat = r.PuntoLlegadaLat,
                    PuntoLlegadaLong = r.PuntoLlegadaLong,
                    PuntoLlegadaNombre = r.PuntoLlegadaNombre,
                    DistanciaKm = r.DistanciaKm,
                    TiempoEstimadoMinutos = r.TiempoEstimadoMinutos,
                    Activa = r.Activa,
                    FechaCreacion = r.FechaCreacion,
                    TotalParadas = r.Paradas.Count,
                    TotalViajes = r.Viajes.Count
                })
                .ToListAsync();

            return Ok(rutas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rutas");
            return StatusCode(500, new { message = "Error al obtener rutas", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener una ruta por ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PlantillaRutaResponseDto>> GetRuta(int id)
    {
        try
        {
            var ruta = await _context.PlantillasRutas
                .Include(r => r.Paradas)
                .Include(r => r.Viajes)
                .Where(r => r.RutaID == id)
                .Select(r => new PlantillaRutaResponseDto
                {
                    RutaID = r.RutaID,
                    CodigoRuta = r.CodigoRuta,
                    NombreRuta = r.NombreRuta,
                    CiudadOrigen = r.CiudadOrigen,
                    CiudadDestino = r.CiudadDestino,
                    PuntoPartidaLat = r.PuntoPartidaLat,
                    PuntoPartidaLong = r.PuntoPartidaLong,
                    PuntoPartidaNombre = r.PuntoPartidaNombre,
                    PuntoLlegadaLat = r.PuntoLlegadaLat,
                    PuntoLlegadaLong = r.PuntoLlegadaLong,
                    PuntoLlegadaNombre = r.PuntoLlegadaNombre,
                    DistanciaKm = r.DistanciaKm,
                    TiempoEstimadoMinutos = r.TiempoEstimadoMinutos,
                    Activa = r.Activa,
                    FechaCreacion = r.FechaCreacion,
                    TotalParadas = r.Paradas.Count,
                    TotalViajes = r.Viajes.Count
                })
                .FirstOrDefaultAsync();

            if (ruta == null)
                return NotFound(new { message = "Ruta no encontrada" });

            return Ok(ruta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ruta {RutaID}", id);
            return StatusCode(500, new { message = "Error al obtener ruta", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear una nueva plantilla de ruta con paradas
    /// </summary>
    [HttpPost]
    [ClRequirePermission(ClAppPermissions.RutasCreate)]
    public async Task<ActionResult<PlantillaRutaResponseDto>> CrearRuta([FromBody] CrearPlantillaRutaDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validar que no exista otra ruta con el mismo código
            var existeCodigo = await _context.PlantillasRutas
                .AnyAsync(r => r.CodigoRuta == dto.CodigoRuta);

            if (existeCodigo)
                return BadRequest(new { message = "Ya existe una ruta con ese código" });

            var ruta = new PlantillaRuta
            {
                CodigoRuta = dto.CodigoRuta,
                NombreRuta = dto.NombreRuta,
                CiudadOrigen = dto.CiudadOrigen,
                CiudadDestino = dto.CiudadDestino,
                PuntoPartidaLat = dto.PuntoPartidaLat,
                PuntoPartidaLong = dto.PuntoPartidaLong,
                PuntoPartidaNombre = dto.PuntoPartidaNombre,
                PuntoLlegadaLat = dto.PuntoLlegadaLat,
                PuntoLlegadaLong = dto.PuntoLlegadaLong,
                PuntoLlegadaNombre = dto.PuntoLlegadaNombre,
                DistanciaKm = dto.DistanciaKm,
                TiempoEstimadoMinutos = dto.TiempoEstimadoMinutos,
                Activa = true,
                FechaCreacion = DateTime.Now,
                CreadoPor = userId
            };

            _context.PlantillasRutas.Add(ruta);
            await _context.SaveChangesAsync();

            // Agregar paradas si se proporcionaron
            if (dto.Paradas != null && dto.Paradas.Any())
            {
                foreach (var paradaDto in dto.Paradas)
                {
                    var parada = new PlantillaParada
                    {
                        PlantillaRutaID = ruta.RutaID,
                        NombreParada = paradaDto.NombreParada,
                        Latitud = paradaDto.Latitud,
                        Longitud = paradaDto.Longitud,
                        Direccion = paradaDto.Direccion,
                        OrdenParada = paradaDto.OrdenParada,
                        TiempoEsperaMinutos = paradaDto.TiempoEsperaMinutos,
                        EsActiva = true
                    };
                    _context.PlantillasParadas.Add(parada);
                }
                await _context.SaveChangesAsync();
            }

            var response = await _context.PlantillasRutas
                .Include(r => r.Paradas)
                .Where(r => r.RutaID == ruta.RutaID)
                .Select(r => new PlantillaRutaResponseDto
                {
                    RutaID = r.RutaID,
                    CodigoRuta = r.CodigoRuta,
                    NombreRuta = r.NombreRuta,
                    CiudadOrigen = r.CiudadOrigen,
                    CiudadDestino = r.CiudadDestino,
                    PuntoPartidaLat = r.PuntoPartidaLat,
                    PuntoPartidaLong = r.PuntoPartidaLong,
                    PuntoPartidaNombre = r.PuntoPartidaNombre,
                    PuntoLlegadaLat = r.PuntoLlegadaLat,
                    PuntoLlegadaLong = r.PuntoLlegadaLong,
                    PuntoLlegadaNombre = r.PuntoLlegadaNombre,
                    DistanciaKm = r.DistanciaKm,
                    TiempoEstimadoMinutos = r.TiempoEstimadoMinutos,
                    Activa = r.Activa,
                    FechaCreacion = r.FechaCreacion,
                    TotalParadas = r.Paradas.Count,
                    TotalViajes = 0
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Ruta {RutaID} creada por usuario {UserId}", ruta.RutaID, userId);

            return CreatedAtAction(nameof(GetRuta), new { id = ruta.RutaID }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear ruta");
            return StatusCode(500, new { message = "Error al crear ruta", error = ex.Message });
        }
    }

    /// <summary>
    /// Activar/Desactivar una ruta
    /// </summary>
    [HttpPut("{id}/toggle")]
    [ClRequirePermission(ClAppPermissions.RutasUpdate)]
    public async Task<ActionResult> ToggleRuta(int id)
    {
        try
        {
            var ruta = await _context.PlantillasRutas.FindAsync(id);
            if (ruta == null)
                return NotFound(new { message = "Ruta no encontrada" });

            ruta.Activa = !ruta.Activa;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ruta {RutaID} {Status}", id, ruta.Activa ? "activada" : "desactivada");

            return Ok(new { message = $"Ruta {(ruta.Activa ? "activada" : "desactivada")} correctamente", activa = ruta.Activa });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de ruta {RutaID}", id);
            return StatusCode(500, new { message = "Error al cambiar estado de ruta", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener paradas de una ruta
    /// </summary>
    [HttpGet("{id}/paradas")]
    [AllowAnonymous]
    public async Task<ActionResult> GetParadasDeRuta(int id)
    {
        try
        {
            var ruta = await _context.PlantillasRutas.FindAsync(id);
            if (ruta == null)
                return NotFound(new { message = "Ruta no encontrada" });

            var paradas = await _context.PlantillasParadas
                .Where(p => p.PlantillaRutaID == id && p.EsActiva)
                .OrderBy(p => p.OrdenParada)
                .Select(p => new
                {
                    p.ParadaID,
                    p.NombreParada,
                    p.Latitud,
                    p.Longitud,
                    p.Direccion,
                    p.OrdenParada,
                    p.TiempoEsperaMinutos,
                    p.EsActiva
                })
                .ToListAsync();

            return Ok(paradas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener paradas de la ruta {RutaID}", id);
            return StatusCode(500, new { message = "Error al obtener paradas", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar una ruta (solo si no tiene viajes)
    /// </summary>
    [HttpDelete("{id}")]
    [ClRequirePermission(ClAppPermissions.RutasDelete)]
    public async Task<ActionResult> EliminarRuta(int id)
    {
        try
        {
            var ruta = await _context.PlantillasRutas
                .Include(r => r.Viajes)
                .FirstOrDefaultAsync(r => r.RutaID == id);

            if (ruta == null)
                return NotFound(new { message = "Ruta no encontrada" });

            // Verificar si tiene viajes asociados
            if (ruta.Viajes.Any())
            {
                return BadRequest(new { message = "No se puede eliminar una ruta que tiene viajes asociados. Desactívela en su lugar." });
            }

            // Eliminar paradas asociadas
            var paradas = await _context.PlantillasParadas
                .Where(p => p.PlantillaRutaID == id)
                .ToListAsync();

            _context.PlantillasParadas.RemoveRange(paradas);
            _context.PlantillasRutas.Remove(ruta);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ruta {RutaID} eliminada", id);

            return Ok(new { message = "Ruta eliminada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar ruta {RutaID}", id);
            return StatusCode(500, new { message = "Error al eliminar ruta", error = ex.Message });
        }
    }
}

