using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Eventos;
using prjBusTix.Model;
using prjBusTix.Security;
using System.Security.Claims;

namespace prjBusTix.Controllers
{
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
        /// <param name="fechaDesde">Filtrar eventos desde esta fecha (opcional)</param>
        /// <param name="fechaHasta">Filtrar eventos hasta esta fecha (opcional)</param>
        /// <param name="ciudad">Filtrar por ciudad (búsqueda parcial, opcional)</param>
        /// <param name="estatus">Filtrar por ID de estatus (opcional)</param>
        /// <param name="soloActivos">Si es true, solo devuelve eventos activos y futuros (default: false)</param>
        /// <returns>Lista de eventos que cumplen con los filtros</returns>
        /// <response code="200">Devuelve la lista de eventos exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<EventoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// Obtener un evento por su ID
        /// </summary>
        /// <param name="id">ID del evento</param>
        /// <returns>Detalles completos del evento incluyendo número de viajes asociados</returns>
        /// <response code="200">Devuelve el evento solicitado</response>
        /// <response code="404">Evento no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EventoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <param name="dto">Datos del evento a crear. Campos requeridos: Nombre y Fecha. HoraInicio debe enviarse como string "HH:mm:ss"</param>
        /// <returns>El evento recién creado con su ID asignado</returns>
        /// <response code="201">Evento creado exitosamente. Retorna el evento con Location header</response>
        /// <response code="400">Datos inválidos o error de validación. Revisa el campo 'errors' en la respuesta</response>
        /// <response code="401">No autenticado. Se requiere token JWT válido</response>
        /// <response code="403">No autorizado. Se requiere permiso 'EventosCreate'</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Ejemplo de request body:
        /// 
        ///     POST /api/Eventos
        ///     {
        ///        "nombre": "Concierto Rock 2025",
        ///        "descripcion": "Gran concierto de rock",
        ///        "tipoEvento": "Concierto",
        ///        "fecha": "2025-12-01T20:00:00Z",
        ///        "horaInicio": "20:00:00",
        ///        "recinto": "Estadio Nacional",
        ///        "direccion": "Av. Principal 123",
        ///        "ciudad": "Ciudad de Mexico",
        ///        "estado": "CDMX",
        ///        "ubicacionLat": 19.432608,
        ///        "ubicacionLong": -99.133209,
        ///        "urlImagen": "https://cdn.example.com/evento.jpg"
        ///     }
        /// </remarks>
        [HttpPost]
        [ClRequirePermission(ClAppPermissions.EventosCreate)]
        [ProducesResponseType(typeof(EventoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EventoResponseDto>> CrearEvento([FromBody] CrearEventoDto dto)
        {
            // Validación del modelo
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors != null && x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => new 
                    { 
                        Field = x.Key, 
                        Error = string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage 
                    }))
                    .ToList();
                
                _logger.LogWarning("Validación fallida al crear evento: {@Errors}", errors);
                return BadRequest(new { message = "Datos inválidos", errors });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                _logger.LogInformation("Creando evento '{Nombre}' por usuario {UserId}", dto.Nombre, userId);

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

                _logger.LogInformation("Evento {EventoID} creado exitosamente por usuario {UserId}", evento.EventoID, userId);

                return CreatedAtAction(nameof(GetEvento), new { id = evento.EventoID }, response);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de BD al crear evento");
                return BadRequest(new { message = "Error al guardar en la base de datos", detail = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear evento");
                return StatusCode(500, new { message = "Error al crear evento", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar un evento existente (actualización parcial)
        /// </summary>
        /// <param name="id">ID del evento a actualizar</param>
        /// <param name="dto">Campos a actualizar. Solo se actualizan los campos proporcionados (no nulos)</param>
        /// <returns>El evento actualizado con todos sus datos</returns>
        /// <response code="200">Evento actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">No autenticado. Se requiere token JWT válido</response>
        /// <response code="403">No autorizado. Se requiere permiso 'EventosUpdate'</response>
        /// <response code="404">Evento no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Este endpoint permite actualización parcial. Solo envía los campos que deseas modificar.
        /// 
        /// Ejemplo de request body (solo actualizar nombre y fecha):
        /// 
        ///     PUT /api/Eventos/5
        ///     {
        ///        "nombre": "Nuevo Nombre del Evento",
        ///        "fecha": "2025-12-15T20:00:00Z"
        ///     }
        /// </remarks>
        [HttpPut("{id}")]
        [ClRequirePermission(ClAppPermissions.EventosUpdate)]
        [ProducesResponseType(typeof(EventoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EventoResponseDto>> ActualizarEvento(int id, [FromBody] ActualizarEventoDto dto)
        {
            // Validar que al menos un campo esté presente para actualizar
            if (string.IsNullOrEmpty(dto.Nombre) && 
                dto.Descripcion == null && 
                dto.TipoEvento == null && 
                !dto.Fecha.HasValue && 
                !dto.HoraInicio.HasValue && 
                dto.Recinto == null && 
                dto.Direccion == null && 
                dto.Ciudad == null && 
                dto.Estado == null && 
                !dto.UbicacionLat.HasValue && 
                !dto.UbicacionLong.HasValue && 
                dto.UrlImagen == null && 
                !dto.Estatus.HasValue)
            {
                return BadRequest(new { message = "Debe proporcionar al menos un campo para actualizar" });
            }

            // Validar ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors != null && x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => new 
                    { 
                        Field = x.Key, 
                        Error = string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage 
                    }))
                    .ToList();
                
                _logger.LogWarning("Validación fallida al actualizar evento {EventoID}: {@Errors}", id, errors);
                return BadRequest(new { message = "Datos inválidos", errors });
            }

            try
            {
                var evento = await _context.Eventos.FindAsync(id);
                if (evento == null)
                    return NotFound(new { message = "Evento no encontrado" });

                _logger.LogInformation("Actualizando evento {EventoID}", id);

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

                _logger.LogInformation("Evento {EventoID} actualizado exitosamente", id);

                return Ok(response);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de BD al actualizar evento {EventoID}", id);
                return BadRequest(new { message = "Error al guardar en la base de datos", detail = dbEx.InnerException?.Message ?? dbEx.Message });
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
        /// <param name="id">ID del evento a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        /// <response code="200">Evento eliminado (desactivado) exitosamente. El estatus cambia a Cancelado (3)</response>
        /// <response code="400">No se puede eliminar. El evento tiene viajes activos asociados</response>
        /// <response code="401">No autenticado. Se requiere token JWT válido</response>
        /// <response code="403">No autorizado. Se requiere permiso 'EventosDelete'</response>
        /// <response code="404">Evento no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Este endpoint realiza un "soft delete" cambiando el estatus del evento a Cancelado (3).
        /// No se puede eliminar un evento si tiene viajes activos asociados.
        /// </remarks>
        [HttpDelete("{id}")]
        [ClRequirePermission(ClAppPermissions.EventosDelete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <param name="id">ID del evento</param>
        /// <returns>Lista de viajes asociados al evento con detalles de ruta, unidad y chofer</returns>
        /// <response code="200">Devuelve la lista de viajes del evento</response>
        /// <response code="404">Evento no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}/viajes")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
}