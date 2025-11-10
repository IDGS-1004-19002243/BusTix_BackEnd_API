using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Eventos;
using prjBusTix.Model;
using prjBusTix.Security;
using System.Security.Claims;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EventosController> _logger;

    public EventosController(AppDbContext context, ILogger<EventosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los eventos con filtros opcionales
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EventoResponseDto>>> GetEventos(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] string? ciudad,
        [FromQuery] int? estatus,
        [FromQuery] bool soloActivos = false)
    {
        try
        {
            var query = _context.Eventos
                .Include(e => e.EstatusNavigation)
                .Include(e => e.Viajes)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(e => e.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(e => e.Fecha <= fechaHasta.Value);

            if (!string.IsNullOrEmpty(ciudad))
                query = query.Where(e => e.Ciudad!.Contains(ciudad));

            if (estatus.HasValue)
                query = query.Where(e => e.Estatus == estatus.Value);

            if (soloActivos)
                query = query.Where(e => e.Estatus == 1 && e.Fecha >= DateTime.Today);

            var eventos = await query
                .OrderByDescending(e => e.Fecha)
                .Select(e => new EventoResponseDto
                {
                    EventoID = e.EventoID,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    TipoEvento = e.TipoEvento,
                    Fecha = e.Fecha,
                    HoraInicio = e.HoraInicio,
                    Recinto = e.Recinto,
                    Direccion = e.Direccion,
                    Ciudad = e.Ciudad,
                    Estado = e.Estado,
                    UbicacionLat = e.UbicacionLat,
                    UbicacionLong = e.UbicacionLong,
                    UrlImagen = e.UrlImagen,
                    Estatus = e.Estatus,
                    EstatusNombre = e.EstatusNavigation.Nombre,
                    FechaCreacion = e.FechaCreacion,
                    CreadoPor = e.CreadoPor,
                    TotalViajes = e.Viajes.Count
                })
                .ToListAsync();

            return Ok(eventos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener eventos");
            return StatusCode(500, new { message = "Error al obtener eventos", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener un evento por ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventoResponseDto>> GetEvento(int id)
    {
        try
        {
            var evento = await _context.Eventos
                .Include(e => e.EstatusNavigation)
                .Include(e => e.Viajes)
                .Where(e => e.EventoID == id)
                .Select(e => new EventoResponseDto
                {
                    EventoID = e.EventoID,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    TipoEvento = e.TipoEvento,
                    Fecha = e.Fecha,
                    HoraInicio = e.HoraInicio,
                    Recinto = e.Recinto,
                    Direccion = e.Direccion,
                    Ciudad = e.Ciudad,
                    Estado = e.Estado,
                    UbicacionLat = e.UbicacionLat,
                    UbicacionLong = e.UbicacionLong,
                    UrlImagen = e.UrlImagen,
                    Estatus = e.Estatus,
                    EstatusNombre = e.EstatusNavigation.Nombre,
                    FechaCreacion = e.FechaCreacion,
                    CreadoPor = e.CreadoPor,
                    TotalViajes = e.Viajes.Count
                })
                .FirstOrDefaultAsync();

            if (evento == null)
                return NotFound(new { message = "Evento no encontrado" });

            return Ok(evento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener evento {EventoID}", id);
            return StatusCode(500, new { message = "Error al obtener evento", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear un nuevo evento
    /// </summary>
    [HttpPost]
    [ClRequirePermission(ClAppPermissions.EventosCreate)]
    public async Task<ActionResult<EventoResponseDto>> CrearEvento([FromBody] CrearEventoDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var evento = new Evento
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                TipoEvento = dto.TipoEvento,
                Fecha = dto.Fecha,
                HoraInicio = dto.HoraInicio,
                Recinto = dto.Recinto,
                Direccion = dto.Direccion,
                Ciudad = dto.Ciudad,
                Estado = dto.Estado,
                UbicacionLat = dto.UbicacionLat,
                UbicacionLong = dto.UbicacionLong,
                UrlImagen = dto.UrlImagen,
                Estatus = 1,
                FechaCreacion = DateTime.Now,
                CreadoPor = userId
            };

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            var response = await _context.Eventos
                .Include(e => e.EstatusNavigation)
                .Where(e => e.EventoID == evento.EventoID)
                .Select(e => new EventoResponseDto
                {
                    EventoID = e.EventoID,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    TipoEvento = e.TipoEvento,
                    Fecha = e.Fecha,
                    HoraInicio = e.HoraInicio,
                    Recinto = e.Recinto,
                    Direccion = e.Direccion,
                    Ciudad = e.Ciudad,
                    Estado = e.Estado,
                    UbicacionLat = e.UbicacionLat,
                    UbicacionLong = e.UbicacionLong,
                    UrlImagen = e.UrlImagen,
                    Estatus = e.Estatus,
                    EstatusNombre = e.EstatusNavigation.Nombre,
                    FechaCreacion = e.FechaCreacion,
                    CreadoPor = e.CreadoPor,
                    TotalViajes = 0
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Evento {EventoID} creado por usuario {UserId}", evento.EventoID, userId);

            return CreatedAtAction(nameof(GetEvento), new { id = evento.EventoID }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear evento");
            return StatusCode(500, new { message = "Error al crear evento", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar un evento existente
    /// </summary>
    [HttpPut("{id}")]
    [ClRequirePermission(ClAppPermissions.EventosUpdate)]
    public async Task<ActionResult<EventoResponseDto>> ActualizarEvento(int id, [FromBody] ActualizarEventoDto dto)
    {
        try
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null)
                return NotFound(new { message = "Evento no encontrado" });

            // Actualizar solo los campos proporcionados
            if (!string.IsNullOrEmpty(dto.Nombre))
                evento.Nombre = dto.Nombre;
            
            if (dto.Descripcion != null)
                evento.Descripcion = dto.Descripcion;
            
            if (dto.TipoEvento != null)
                evento.TipoEvento = dto.TipoEvento;
            
            if (dto.Fecha.HasValue)
                evento.Fecha = dto.Fecha.Value;
            
            if (dto.HoraInicio.HasValue)
                evento.HoraInicio = dto.HoraInicio;
            
            if (dto.Recinto != null)
                evento.Recinto = dto.Recinto;
            
            if (dto.Direccion != null)
                evento.Direccion = dto.Direccion;
            
            if (dto.Ciudad != null)
                evento.Ciudad = dto.Ciudad;
            
            if (dto.Estado != null)
                evento.Estado = dto.Estado;
            
            if (dto.UbicacionLat.HasValue)
                evento.UbicacionLat = dto.UbicacionLat;
            
            if (dto.UbicacionLong.HasValue)
                evento.UbicacionLong = dto.UbicacionLong;
            
            if (dto.UrlImagen != null)
                evento.UrlImagen = dto.UrlImagen;
            
            if (dto.Estatus.HasValue)
                evento.Estatus = dto.Estatus.Value;

            await _context.SaveChangesAsync();

            var response = await _context.Eventos
                .Include(e => e.EstatusNavigation)
                .Include(e => e.Viajes)
                .Where(e => e.EventoID == id)
                .Select(e => new EventoResponseDto
                {
                    EventoID = e.EventoID,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    TipoEvento = e.TipoEvento,
                    Fecha = e.Fecha,
                    HoraInicio = e.HoraInicio,
                    Recinto = e.Recinto,
                    Direccion = e.Direccion,
                    Ciudad = e.Ciudad,
                    Estado = e.Estado,
                    UbicacionLat = e.UbicacionLat,
                    UbicacionLong = e.UbicacionLong,
                    UrlImagen = e.UrlImagen,
                    Estatus = e.Estatus,
                    EstatusNombre = e.EstatusNavigation.Nombre,
                    FechaCreacion = e.FechaCreacion,
                    CreadoPor = e.CreadoPor,
                    TotalViajes = e.Viajes.Count
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Evento {EventoID} actualizado", id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar evento {EventoID}", id);
            return StatusCode(500, new { message = "Error al actualizar evento", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar (desactivar) un evento
    /// </summary>
    [HttpDelete("{id}")]
    [ClRequirePermission(ClAppPermissions.EventosDelete)]
    public async Task<ActionResult> EliminarEvento(int id)
    {
        try
        {
            var evento = await _context.Eventos
                .Include(e => e.Viajes)
                .FirstOrDefaultAsync(e => e.EventoID == id);

            if (evento == null)
                return NotFound(new { message = "Evento no encontrado" });

            // Verificar si tiene viajes activos
            if (evento.Viajes.Any(v => v.Estatus == 1))
            {
                return BadRequest(new { message = "No se puede eliminar un evento con viajes activos" });
            }

            // Soft delete - cambiar estatus
            evento.Estatus = 3; // Cancelado
            await _context.SaveChangesAsync();

            _logger.LogInformation("Evento {EventoID} eliminado (desactivado)", id);

            return Ok(new { message = "Evento eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar evento {EventoID}", id);
            return StatusCode(500, new { message = "Error al eliminar evento", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener viajes de un evento específico
    /// </summary>
    [HttpGet("{id}/viajes")]
    [AllowAnonymous]
    public async Task<ActionResult> GetViajesDeEvento(int id)
    {
        try
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null)
                return NotFound(new { message = "Evento no encontrado" });

            var viajes = await _context.Viajes
                .Include(v => v.PlantillaRuta)
                .Include(v => v.Unidad)
                .Include(v => v.Chofer)
                .Include(v => v.EstatusNavigation)
                .Where(v => v.EventoID == id)
                .OrderBy(v => v.FechaSalida)
                .Select(v => new
                {
                    v.ViajeID,
                    v.CodigoViaje,
                    v.TipoViaje,
                    v.FechaSalida,
                    v.FechaLlegadaEstimada,
                    RutaNombre = v.PlantillaRuta.NombreRuta,
                    CiudadOrigen = v.PlantillaRuta.CiudadOrigen,
                    CiudadDestino = v.PlantillaRuta.CiudadDestino,
                    UnidadPlacas = v.Unidad != null ? v.Unidad.Placas : null,
                    ChoferNombre = v.Chofer != null ? v.Chofer.NombreCompleto : null,
                    v.CupoTotal,
                    v.AsientosDisponibles,
                    v.AsientosVendidos,
                    v.PrecioBase,
                    v.VentasAbiertas,
                    v.Estatus,
                    EstatusNombre = v.EstatusNavigation.Nombre
                })
                .ToListAsync();

            return Ok(viajes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener viajes del evento {EventoID}", id);
            return StatusCode(500, new { message = "Error al obtener viajes", error = ex.Message });
        }
    }
}

