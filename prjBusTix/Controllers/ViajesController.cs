using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Viajes;
using prjBusTix.Dto.ViajeStaff;
using prjBusTix.Model;
using prjBusTix.Security;
using System.Security.Claims;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ViajesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ViajesController> _logger;

    public ViajesController(AppDbContext context, ILogger<ViajesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los viajes con filtros opcionales
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ViajeResponseDto>>> GetViajes(
        [FromQuery] int? eventoId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] string? ciudadOrigen,
        [FromQuery] string? ciudadDestino,
        [FromQuery] bool? soloDisponibles = false,
        [FromQuery] int? estatus = null)
    {
        try
        {
            var query = _context.Viajes
                .Include(v => v.Evento)
                .Include(v => v.PlantillaRuta)
                .Include(v => v.Unidad)
                .Include(v => v.Chofer)
                .Include(v => v.EstatusNavigation)
                .Include(v => v.Paradas)
                .Include(v => v.Staff)
                .Include(v => v.Incidencias)
                .AsQueryable();

            if (eventoId.HasValue)
                query = query.Where(v => v.EventoID == eventoId.Value);

            if (fechaDesde.HasValue)
                query = query.Where(v => v.FechaSalida >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(v => v.FechaSalida <= fechaHasta.Value);

            if (!string.IsNullOrEmpty(ciudadOrigen))
                query = query.Where(v => v.PlantillaRuta.CiudadOrigen.Contains(ciudadOrigen));

            if (!string.IsNullOrEmpty(ciudadDestino))
                query = query.Where(v => v.PlantillaRuta.CiudadDestino.Contains(ciudadDestino));

            if (soloDisponibles == true)
                query = query.Where(v => v.AsientosDisponibles > 0 && v.VentasAbiertas && v.Estatus == 1);

            if (estatus.HasValue)
                query = query.Where(v => v.Estatus == estatus.Value);

            var viajes = await query
                .OrderBy(v => v.FechaSalida)
                .Select(v => new ViajeResponseDto
                {
                    ViajeID = v.ViajeID,
                    CodigoViaje = v.CodigoViaje,
                    TipoViaje = v.TipoViaje,
                    EventoID = v.EventoID,
                    EventoNombre = v.Evento.Nombre,
                    EventoFecha = v.Evento.Fecha,
                    PlantillaRutaID = v.PlantillaRutaID,
                    RutaNombre = v.PlantillaRuta.NombreRuta,
                    CiudadOrigen = v.PlantillaRuta.CiudadOrigen,
                    CiudadDestino = v.PlantillaRuta.CiudadDestino,
                    UnidadID = v.UnidadID,
                    UnidadPlacas = v.Unidad != null ? v.Unidad.Placas : null,
                    UnidadModelo = v.Unidad != null ? v.Unidad.Modelo : null,
                    ChoferID = v.ChoferID,
                    ChoferNombre = v.Chofer != null ? v.Chofer.NombreCompleto : null,
                    FechaSalida = v.FechaSalida,
                    FechaLlegadaEstimada = v.FechaLlegadaEstimada,
                    CupoTotal = v.CupoTotal,
                    AsientosDisponibles = v.AsientosDisponibles,
                    AsientosVendidos = v.AsientosVendidos,
                    PrecioBase = v.PrecioBase,
                    CargoServicio = v.CargoServicio,
                    VentasAbiertas = v.VentasAbiertas,
                    Estatus = v.Estatus,
                    EstatusNombre = v.EstatusNavigation.Nombre,
                    FechaCreacion = v.FechaCreacion,
                    TotalParadas = v.Paradas.Count,
                    TotalStaff = v.Staff.Count,
                    TotalIncidencias = v.Incidencias.Count
                })
                .ToListAsync();

            return Ok(viajes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener viajes");
            return StatusCode(500, new { message = "Error al obtener viajes", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener los viajes asignados al usuario actual (Chofer o Staff)
    /// GET /api/viajes/mis-viajes
    /// </summary>
    [HttpGet("mis-viajes")]
    public async Task<ActionResult<IEnumerable<ViajeResponseDto>>> GetMisViajes()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Determinar si es chofer o staff (o ambos)
            // Buscamos viajes donde sea chofer
            var viajesComoChofer = await _context.Viajes
                .Include(v => v.Evento)
                .Include(v => v.PlantillaRuta)
                .Include(v => v.Unidad)
                .Include(v => v.Chofer)
                .Include(v => v.EstatusNavigation)
                .Include(v => v.Paradas)
                .Include(v => v.Staff)
                .Include(v => v.Incidencias)
                .Where(v => v.ChoferID == userId)
                .ToListAsync();

            // Buscamos viajes donde sea staff
            var viajesComoStaff = await _context.ViajesStaff
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.Evento)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.Unidad)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.Chofer)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.EstatusNavigation)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.Paradas)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.Staff)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.Incidencias)
                .Where(vs => vs.StaffID == userId)
                .Select(vs => vs.Viaje)
                .ToListAsync();

            // Unir y eliminar duplicados
            var todosMisViajes = viajesComoChofer
                .Union(viajesComoStaff)
                .OrderBy(v => v.FechaSalida)
                .Select(v => new ViajeResponseDto
                {
                    ViajeID = v.ViajeID,
                    CodigoViaje = v.CodigoViaje,
                    TipoViaje = v.TipoViaje,
                    EventoID = v.EventoID,
                    EventoNombre = v.Evento.Nombre,
                    EventoFecha = v.Evento.Fecha,
                    PlantillaRutaID = v.PlantillaRutaID,
                    RutaNombre = v.PlantillaRuta.NombreRuta,
                    CiudadOrigen = v.PlantillaRuta.CiudadOrigen,
                    CiudadDestino = v.PlantillaRuta.CiudadDestino,
                    UnidadID = v.UnidadID,
                    UnidadPlacas = v.Unidad != null ? v.Unidad.Placas : null,
                    UnidadModelo = v.Unidad != null ? v.Unidad.Modelo : null,
                    ChoferID = v.ChoferID,
                    ChoferNombre = v.Chofer != null ? v.Chofer.NombreCompleto : null,
                    FechaSalida = v.FechaSalida,
                    FechaLlegadaEstimada = v.FechaLlegadaEstimada,
                    CupoTotal = v.CupoTotal,
                    AsientosDisponibles = v.AsientosDisponibles,
                    AsientosVendidos = v.AsientosVendidos,
                    PrecioBase = v.PrecioBase,
                    CargoServicio = v.CargoServicio,
                    VentasAbiertas = v.VentasAbiertas,
                    Estatus = v.Estatus,
                    EstatusNombre = v.EstatusNavigation.Nombre,
                    FechaCreacion = v.FechaCreacion,
                    TotalParadas = v.Paradas.Count,
                    TotalStaff = v.Staff.Count,
                    TotalIncidencias = v.Incidencias.Count
                })
                .ToList();

            return Ok(todosMisViajes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mis viajes");
            return StatusCode(500, new { message = "Error al obtener mis viajes", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener un viaje por ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ViajeResponseDto>> GetViaje(int id)
    {
        try
        {
            var viaje = await _context.Viajes
                .Include(v => v.Evento)
                .Include(v => v.PlantillaRuta)
                .Include(v => v.Unidad)
                .Include(v => v.Chofer)
                .Include(v => v.EstatusNavigation)
                .Include(v => v.Paradas)
                .Include(v => v.Staff)
                .Include(v => v.Incidencias)
                .Where(v => v.ViajeID == id)
                .Select(v => new ViajeResponseDto
                {
                    ViajeID = v.ViajeID,
                    CodigoViaje = v.CodigoViaje,
                    TipoViaje = v.TipoViaje,
                    EventoID = v.EventoID,
                    EventoNombre = v.Evento.Nombre,
                    EventoFecha = v.Evento.Fecha,
                    PlantillaRutaID = v.PlantillaRutaID,
                    RutaNombre = v.PlantillaRuta.NombreRuta,
                    CiudadOrigen = v.PlantillaRuta.CiudadOrigen,
                    CiudadDestino = v.PlantillaRuta.CiudadDestino,
                    UnidadID = v.UnidadID,
                    UnidadPlacas = v.Unidad != null ? v.Unidad.Placas : null,
                    UnidadModelo = v.Unidad != null ? v.Unidad.Modelo : null,
                    ChoferID = v.ChoferID,
                    ChoferNombre = v.Chofer != null ? v.Chofer.NombreCompleto : null,
                    FechaSalida = v.FechaSalida,
                    FechaLlegadaEstimada = v.FechaLlegadaEstimada,
                    CupoTotal = v.CupoTotal,
                    AsientosDisponibles = v.AsientosDisponibles,
                    AsientosVendidos = v.AsientosVendidos,
                    PrecioBase = v.PrecioBase,
                    CargoServicio = v.CargoServicio,
                    VentasAbiertas = v.VentasAbiertas,
                    Estatus = v.Estatus,
                    EstatusNombre = v.EstatusNavigation.Nombre,
                    FechaCreacion = v.FechaCreacion,
                    TotalParadas = v.Paradas.Count,
                    TotalStaff = v.Staff.Count,
                    TotalIncidencias = v.Incidencias.Count
                })
                .FirstOrDefaultAsync();

            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            return Ok(viaje);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al obtener viaje", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener detalle completo del viaje para el cliente (con paradas y precios)
    /// GET /api/viajes/{id}/detalle-cliente
    /// </summary>
    [HttpGet("{id}/detalle-cliente")]
    [AllowAnonymous]
    public async Task<ActionResult<ViajeDetalleClienteDto>> GetViajeDetalleCliente(int id)
    {
        try
        {
            var viaje = await _context.Viajes
                .Include(v => v.Evento)
                .Include(v => v.PlantillaRuta)
                .Include(v => v.Unidad)
                .Include(v => v.Chofer)
                .Include(v => v.Paradas.OrderBy(p => p.OrdenParada))
                .FirstOrDefaultAsync(v => v.ViajeID == id);

            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            // Obtener precios específicos por parada
            var preciosParadas = await _context.PreciosParada
                .Where(p => p.ViajeID == id && p.EsActivo)
                .ToDictionaryAsync(p => p.ParadaViajeID, p => p);

            // Calcular precio mínimo y máximo
            var precios = new List<decimal>();
            foreach (var parada in viaje.Paradas)
            {
                decimal precioBase = viaje.PrecioBase;
                decimal cargoServicio = viaje.CargoServicio;

                if (preciosParadas.TryGetValue(parada.ParadaViajeID, out var precioParada))
                {
                    precioBase = precioParada.PrecioBase;
                    cargoServicio = precioParada.CargoServicio;
                }

                decimal subtotal = precioBase + cargoServicio;
                decimal iva = subtotal * 0.16m;
                decimal total = subtotal + iva;
                precios.Add(total);
            }

            decimal precioDesde = precios.Any() ? precios.Min() : viaje.PrecioBase;
            decimal precioHasta = precios.Any() ? precios.Max() : viaje.PrecioBase;

            // Construir DTO de paradas con precios
            var paradasConPrecio = viaje.Paradas.Select(p =>
            {
                decimal precioBase = viaje.PrecioBase;
                decimal cargoServicio = viaje.CargoServicio;

                if (preciosParadas.TryGetValue(p.ParadaViajeID, out var precioParada))
                {
                    precioBase = precioParada.PrecioBase;
                    cargoServicio = precioParada.CargoServicio;
                }

                decimal subtotal = precioBase + cargoServicio;
                decimal iva = subtotal * 0.16m;
                decimal total = subtotal + iva;

                return new ParadaConPrecioDto
                {
                    ParadaViajeID = p.ParadaViajeID,
                    NombreParada = p.NombreParada,
                    Direccion = p.Direccion ?? "",
                    Latitud = p.Latitud,
                    Longitud = p.Longitud,
                    OrdenParada = p.OrdenParada,
                    HoraEstimadaLlegada = p.HoraEstimadaLlegada,
                    TiempoEsperaMinutos = p.TiempoEsperaMinutos,
                    PrecioBase = precioBase,
                    CargoServicio = cargoServicio,
                    PrecioTotal = precioBase + cargoServicio,
                    IVA = iva,
                    TotalAPagar = total,
                    AsientosDisponibles = viaje.AsientosDisponibles
                };
            }).ToList();

            var duracion = viaje.FechaLlegadaEstimada.HasValue
                ? (int)(viaje.FechaLlegadaEstimada.Value - viaje.FechaSalida).TotalHours
                : 0;

            var detalle = new ViajeDetalleClienteDto
            {
                ViajeID = viaje.ViajeID,
                CodigoViaje = viaje.CodigoViaje,
                TipoViaje = viaje.TipoViaje,
                EventoID = viaje.EventoID,
                EventoNombre = viaje.Evento.Nombre,
                EventoDescripcion = viaje.Evento.Descripcion ?? "",
                EventoFecha = viaje.Evento.Fecha,
                EventoRecinto = viaje.Evento.Recinto ?? "",
                EventoCiudad = viaje.Evento.Ciudad ?? "",
                EventoUrlImagen = viaje.Evento.UrlImagen,
                RutaNombre = viaje.PlantillaRuta.NombreRuta,
                CiudadOrigen = viaje.PlantillaRuta.CiudadOrigen,
                CiudadDestino = viaje.PlantillaRuta.CiudadDestino,
                FechaSalida = viaje.FechaSalida,
                FechaLlegadaEstimada = viaje.FechaLlegadaEstimada,
                DuracionEstimadaHoras = duracion,
                CupoTotal = viaje.CupoTotal,
                AsientosDisponibles = viaje.AsientosDisponibles,
                AsientosVendidos = viaje.AsientosVendidos,
                VentasAbiertas = viaje.VentasAbiertas,
                PrecioBase = viaje.PrecioBase,
                CargoServicio = viaje.CargoServicio,
                PrecioDesde = precioDesde,
                PrecioHasta = precioHasta,
                UnidadModelo = viaje.Unidad?.Modelo,
                UnidadPlacas = viaje.Unidad?.Placas,
                CapacidadUnidad = viaje.Unidad?.CapacidadAsientos,
                ChoferNombre = viaje.Chofer?.NombreCompleto,
                Paradas = paradasConPrecio,
                TieneServicioWifi = true, // TODO: Agregar estos campos al modelo Unidad
                TieneAireAcondicionado = true,
                TieneBaño = true
            };

            return Ok(detalle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle del viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al obtener detalle del viaje", error = ex.Message });
        }
    }

    /// <summary>
    /// Verificar disponibilidad de unidad y chofer antes de crear el viaje
    /// </summary>
    [HttpPost("verificar-disponibilidad")]
    [ClRequirePermission(ClAppPermissions.ViajesCreate)]
    public async Task<ActionResult<DisponibilidadResponseDto>> VerificarDisponibilidad([FromBody] CrearViajeDto dto)
    {
        return await VerificarDisponibilidadInternal(dto);
    }

    /// <summary>
    /// Verificar disponibilidad (versión GET para compatibilidad con frontend)
    /// </summary>
    [HttpGet("verificar-disponibilidad")]
    [ClRequirePermission(ClAppPermissions.ViajesCreate)]
    public async Task<ActionResult<DisponibilidadResponseDto>> VerificarDisponibilidadGet([FromQuery] CrearViajeDto dto)
    {
        return await VerificarDisponibilidadInternal(dto);
    }

    private async Task<ActionResult<DisponibilidadResponseDto>> VerificarDisponibilidadInternal(CrearViajeDto dto)
    {
        try
        {
            var response = new DisponibilidadResponseDto { EstaDisponible = true };
            var conflictos = new List<ConflictoDto>();

            // Validar que la plantilla de ruta existe para obtener tiempo estimado
            var plantillaRuta = await _context.PlantillasRutas.FindAsync(dto.PlantillaRutaID);
            if (plantillaRuta == null)
            {
                return BadRequest(new { message = "La plantilla de ruta especificada no existe" });
            }

            // Calcular ventana de tiempo
            var minutosFallback = (plantillaRuta.TiempoEstimadoMinutos ?? 240);
            var nuevoInicio = dto.FechaSalida;
            var nuevoFin = dto.FechaLlegadaEstimada ?? dto.FechaSalida.AddMinutes(minutosFallback);

            // Verificar conflictos de Unidad
            if (dto.UnidadID.HasValue)
            {
                var viajesConflicto = await _context.Viajes
                    .Include(v => v.Evento)
                    .Include(v => v.PlantillaRuta)
                    .Where(v =>
                        v.UnidadID == dto.UnidadID.Value && 
                        v.Estatus != 3 && // No cancelado
                        v.FechaSalida <= nuevoFin &&
                        (v.FechaLlegadaEstimada ?? v.FechaSalida.AddMinutes(240)) >= nuevoInicio)
                    .ToListAsync();

                foreach (var v in viajesConflicto)
                {
                    conflictos.Add(new ConflictoDto
                    {
                        ViajeID = v.ViajeID,
                        CodigoViaje = v.CodigoViaje,
                        FechaSalida = v.FechaSalida,
                        FechaLlegadaEstimada = v.FechaLlegadaEstimada,
                        EventoNombre = v.Evento?.Nombre ?? "Sin Evento",
                        RutaNombre = v.PlantillaRuta?.NombreRuta ?? "Sin Ruta"
                    });
                }
            }

            // Verificar conflictos de Chofer
            if (!string.IsNullOrEmpty(dto.ChoferID))
            {
                var viajesConflicto = await _context.Viajes
                    .Include(v => v.Evento)
                    .Include(v => v.PlantillaRuta)
                    .Where(v =>
                        v.ChoferID == dto.ChoferID && 
                        v.Estatus != 3 && // No cancelado
                        v.FechaSalida <= nuevoFin &&
                        (v.FechaLlegadaEstimada ?? v.FechaSalida.AddMinutes(240)) >= nuevoInicio)
                    .ToListAsync();

                // Agregar solo si no están ya en la lista (para evitar duplicados si coincide unidad y chofer)
                foreach (var v in viajesConflicto)
                {
                    if (!conflictos.Any(c => c.ViajeID == v.ViajeID))
                    {
                        conflictos.Add(new ConflictoDto
                        {
                            ViajeID = v.ViajeID,
                            CodigoViaje = v.CodigoViaje,
                            FechaSalida = v.FechaSalida,
                            FechaLlegadaEstimada = v.FechaLlegadaEstimada,
                            EventoNombre = v.Evento?.Nombre ?? "Sin Evento",
                            RutaNombre = v.PlantillaRuta?.NombreRuta ?? "Sin Ruta"
                        });
                    }
                }
            }

            if (conflictos.Any())
            {
                response.EstaDisponible = false;
                response.Mensaje = "Se encontraron conflictos de agenda";
                response.Conflictos = conflictos;
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad");
            return StatusCode(500, new { message = "Error al verificar disponibilidad", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear un nuevo viaje
    /// </summary>
    [HttpPost]
    [ClRequirePermission(ClAppPermissions.ViajesCreate)]
    public async Task<ActionResult<ViajeResponseDto>> CrearViaje([FromBody] CrearViajeDto dto)
    {
        try
        {
            // Validar ModelState y retornar errores detallados
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                
                return BadRequest(new
                {
                    success = false,
                    message = "Error de validación en los datos proporcionados",
                    errors = errors
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validar que el evento existe
            var evento = await _context.Eventos.FindAsync(dto.EventoID);
            if (evento == null)
                return BadRequest(new { 
                    success = false,
                    message = $"El evento con ID {dto.EventoID} no existe en la base de datos",
                    field = "EventoID",
                    receivedValue = dto.EventoID
                });

            // Validar que la plantilla de ruta existe
            var plantillaRuta = await _context.PlantillasRutas.FindAsync(dto.PlantillaRutaID);
            if (plantillaRuta == null || !plantillaRuta.Activa)
                return BadRequest(new { 
                    success = false,
                    message = plantillaRuta == null 
                        ? $"La plantilla de ruta con ID {dto.PlantillaRutaID} no existe en la base de datos"
                        : $"La plantilla de ruta con ID {dto.PlantillaRutaID} está inactiva",
                    field = "PlantillaRutaID",
                    receivedValue = dto.PlantillaRutaID
                });

            // Validar unidad si se proporciona
            if (dto.UnidadID.HasValue)
            {
                var unidad = await _context.Unidades.FindAsync(dto.UnidadID.Value);
                if (unidad == null)
                    return BadRequest(new { 
                        success = false,
                        message = $"La unidad con ID {dto.UnidadID.Value} no existe en la base de datos",
                        field = "UnidadID",
                        receivedValue = dto.UnidadID.Value
                    });
                if (unidad.Estatus != 1)
                    return BadRequest(new { 
                        success = false,
                        message = $"La unidad con ID {dto.UnidadID.Value} no está disponible (Estatus: {unidad.Estatus})",
                        field = "UnidadID",
                        receivedValue = dto.UnidadID.Value
                    });
            }

            // Validar chofer si se proporciona
            if (!string.IsNullOrEmpty(dto.ChoferID))
            {
                var chofer = await _context.Users.FindAsync(dto.ChoferID);
                if (chofer == null)
                    return BadRequest(new { 
                        success = false,
                        message = $"El chofer con ID '{dto.ChoferID}' no existe en la base de datos",
                        field = "ChoferID",
                        receivedValue = dto.ChoferID
                    });
                if (chofer.Estatus != 1)
                    return BadRequest(new { 
                        success = false,
                        message = $"El chofer con ID '{dto.ChoferID}' no está activo (Estatus: {chofer.Estatus})",
                        field = "ChoferID",
                        receivedValue = dto.ChoferID
                    });
            }

            // Calcular ventana de tiempo del nuevo viaje
            var minutosFallback = (plantillaRuta.TiempoEstimadoMinutos ?? 240);
            var nuevoInicio = dto.FechaSalida;
            var nuevoFin = dto.FechaLlegadaEstimada ?? dto.FechaSalida.AddMinutes(minutosFallback);

            // Conflicto de agenda - Unidad
            if (dto.UnidadID.HasValue)
            {
                var conflictoUnidad = await _context.Viajes.AnyAsync(v =>
                    v.UnidadID == dto.UnidadID.Value && v.Estatus != 3 &&
                    v.ViajeID != 0 && // placeholder, no aplica para creación
                    v.FechaSalida <= nuevoFin &&
                    (v.FechaLlegadaEstimada ?? v.FechaSalida.AddMinutes(240)) >= nuevoInicio);
                if (conflictoUnidad)
                {
                    return BadRequest(new { message = "La unidad seleccionada tiene un conflicto de agenda en ese horario" });
                }
            }

            // Conflicto de agenda - Chofer
            if (!string.IsNullOrEmpty(dto.ChoferID))
            {
                var conflictoChofer = await _context.Viajes.AnyAsync(v =>
                    v.ChoferID == dto.ChoferID && v.Estatus != 3 &&
                    v.FechaSalida <= nuevoFin &&
                    (v.FechaLlegadaEstimada ?? v.FechaSalida.AddMinutes(240)) >= nuevoInicio);
                if (conflictoChofer)
                {
                    return BadRequest(new { message = "El chofer seleccionado tiene un conflicto de agenda en ese horario" });
                }
            }

            // Generar código único de viaje
            var codigoViaje = $"V{DateTime.Now:yyyyMMddHHmmss}";

            var viaje = new Viaje
            {
                EventoID = dto.EventoID,
                PlantillaRutaID = dto.PlantillaRutaID,
                UnidadID = dto.UnidadID,
                ChoferID = dto.ChoferID,
                CodigoViaje = codigoViaje,
                TipoViaje = dto.TipoViaje,
                FechaSalida = dto.FechaSalida,
                FechaLlegadaEstimada = dto.FechaLlegadaEstimada,
                CupoTotal = dto.CupoTotal,
                AsientosDisponibles = dto.CupoTotal,
                AsientosVendidos = 0,
                PrecioBase = dto.PrecioBase,
                CargoServicio = dto.CargoServicio,
                VentasAbiertas = dto.VentasAbiertas,
                Estatus = 1,
                FechaCreacion = DateTime.Now,
                CreadoPor = userId
            };

            _context.Viajes.Add(viaje);
            await _context.SaveChangesAsync();

            // Copiar paradas de la plantilla al viaje
            var paradasPlantilla = await _context.PlantillasParadas
                .Where(p => p.PlantillaRutaID == dto.PlantillaRutaID)
                .OrderBy(p => p.OrdenParada)
                .ToListAsync();

            foreach (var paradaPlantilla in paradasPlantilla)
            {
                var paradaViaje = new ParadaViaje
                {
                    ViajeID = viaje.ViajeID,
                    PlantillaParadaID = paradaPlantilla.ParadaID,
                    NombreParada = paradaPlantilla.NombreParada,
                    Direccion = paradaPlantilla.Direccion,
                    Latitud = paradaPlantilla.Latitud,
                    Longitud = paradaPlantilla.Longitud,
                    OrdenParada = paradaPlantilla.OrdenParada,
                    EsActiva = true
                };
                _context.ParadasViaje.Add(paradaViaje);
            }

            await _context.SaveChangesAsync();

            var response = await _context.Viajes
                .Include(v => v.Evento)
                .Include(v => v.PlantillaRuta)
                .Include(v => v.Unidad)
                .Include(v => v.Chofer)
                .Include(v => v.EstatusNavigation)
                .Include(v => v.Paradas)
                .Where(v => v.ViajeID == viaje.ViajeID)
                .Select(v => new ViajeResponseDto
                {
                    ViajeID = v.ViajeID,
                    CodigoViaje = v.CodigoViaje,
                    TipoViaje = v.TipoViaje,
                    EventoID = v.EventoID,
                    EventoNombre = v.Evento.Nombre,
                    EventoFecha = v.Evento.Fecha,
                    PlantillaRutaID = v.PlantillaRutaID,
                    RutaNombre = v.PlantillaRuta.NombreRuta,
                    CiudadOrigen = v.PlantillaRuta.CiudadOrigen,
                    CiudadDestino = v.PlantillaRuta.CiudadDestino,
                    UnidadID = v.UnidadID,
                    UnidadPlacas = v.Unidad != null ? v.Unidad.Placas : null,
                    UnidadModelo = v.Unidad != null ? v.Unidad.Modelo : null,
                    ChoferID = v.ChoferID,
                    ChoferNombre = v.Chofer != null ? v.Chofer.NombreCompleto : null,
                    FechaSalida = v.FechaSalida,
                    FechaLlegadaEstimada = v.FechaLlegadaEstimada,
                    CupoTotal = v.CupoTotal,
                    AsientosDisponibles = v.AsientosDisponibles,
                    AsientosVendidos = v.AsientosVendidos,
                    PrecioBase = v.PrecioBase,
                    CargoServicio = v.CargoServicio,
                    VentasAbiertas = v.VentasAbiertas,
                    Estatus = v.Estatus,
                    EstatusNombre = v.EstatusNavigation.Nombre,
                    FechaCreacion = v.FechaCreacion,
                    TotalParadas = v.Paradas.Count,
                    TotalStaff = 0,
                    TotalIncidencias = 0
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Viaje {ViajeID} creado por usuario {UserId}", viaje.ViajeID, userId);

            return CreatedAtAction(nameof(GetViaje), new { id = viaje.ViajeID }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear viaje");
            return StatusCode(500, new { message = "Error al crear viaje", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar un viaje existente
    /// </summary>
    [HttpPut("{id}")]
    [ClRequirePermission(ClAppPermissions.ViajesUpdate)]
    public async Task<ActionResult<ViajeResponseDto>> ActualizarViaje(int id, [FromBody] ActualizarViajeDto dto)
    {
        try
        {
            // Validar ModelState y retornar errores detallados
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                
                return BadRequest(new
                {
                    success = false,
                    message = "Error de validación en los datos proporcionados",
                    errors = errors
                });
            }

            var viaje = await _context.Viajes.FindAsync(id);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            // No permitir actualizar viajes ya iniciados o terminados
            if (viaje.Estatus > 2)
                return BadRequest(new { message = "No se puede modificar un viaje que ya inici� o termin�" });

            if (dto.FechaLlegadaEstimada.HasValue)
                viaje.FechaLlegadaEstimada = dto.FechaLlegadaEstimada;

            if (dto.PrecioBase.HasValue)
                viaje.PrecioBase = dto.PrecioBase.Value;

            if (dto.CargoServicio.HasValue)
                viaje.CargoServicio = dto.CargoServicio.Value;

            if (dto.VentasAbiertas.HasValue)
                viaje.VentasAbiertas = dto.VentasAbiertas.Value;

            if (dto.Estatus.HasValue)
                viaje.Estatus = dto.Estatus.Value;

            await _context.SaveChangesAsync();

            var response = await _context.Viajes
                .Include(v => v.Evento)
                .Include(v => v.PlantillaRuta)
                .Include(v => v.Unidad)
                .Include(v => v.Chofer)
                .Include(v => v.EstatusNavigation)
                .Include(v => v.Paradas)
                .Include(v => v.Staff)
                .Include(v => v.Incidencias)
                .Where(v => v.ViajeID == id)
                .Select(v => new ViajeResponseDto
                {
                    ViajeID = v.ViajeID,
                    CodigoViaje = v.CodigoViaje,
                    TipoViaje = v.TipoViaje,
                    EventoID = v.EventoID,
                    EventoNombre = v.Evento.Nombre,
                    EventoFecha = v.Evento.Fecha,
                    PlantillaRutaID = v.PlantillaRutaID,
                    RutaNombre = v.PlantillaRuta.NombreRuta,
                    CiudadOrigen = v.PlantillaRuta.CiudadOrigen,
                    CiudadDestino = v.PlantillaRuta.CiudadDestino,
                    UnidadID = v.UnidadID,
                    UnidadPlacas = v.Unidad != null ? v.Unidad.Placas : null,
                    UnidadModelo = v.Unidad != null ? v.Unidad.Modelo : null,
                    ChoferID = v.ChoferID,
                    ChoferNombre = v.Chofer != null ? v.Chofer.NombreCompleto : null,
                    FechaSalida = v.FechaSalida,
                    FechaLlegadaEstimada = v.FechaLlegadaEstimada,
                    CupoTotal = v.CupoTotal,
                    AsientosDisponibles = v.AsientosDisponibles,
                    AsientosVendidos = v.AsientosVendidos,
                    PrecioBase = v.PrecioBase,
                    CargoServicio = v.CargoServicio,
                    VentasAbiertas = v.VentasAbiertas,
                    Estatus = v.Estatus,
                    EstatusNombre = v.EstatusNavigation.Nombre,
                    FechaCreacion = v.FechaCreacion,
                    TotalParadas = v.Paradas.Count,
                    TotalStaff = v.Staff.Count,
                    TotalIncidencias = v.Incidencias.Count
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Viaje {ViajeID} actualizado", id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al actualizar viaje", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar (cancelar) un viaje
    /// </summary>
    [HttpDelete("{id}")]
    [ClRequirePermission(ClAppPermissions.ViajesDelete)]
    public async Task<ActionResult> EliminarViaje(int id)
    {
        try
        {
            var viaje = await _context.Viajes
                .Include(v => v.Boletos)
                .FirstOrDefaultAsync(v => v.ViajeID == id);

            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            // Verificar si tiene boletos vendidos
            if (viaje.AsientosVendidos > 0)
            {
                return BadRequest(new { message = "No se puede eliminar un viaje con boletos vendidos. Debe cancelarlo en su lugar." });
            }

            // Soft delete - cambiar estatus
            viaje.Estatus = 3; // Cancelado
            viaje.VentasAbiertas = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Viaje {ViajeID} eliminado (cancelado)", id);

            return Ok(new { message = "Viaje cancelado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al eliminar viaje", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener paradas de un viaje
    /// </summary>
    [HttpGet("{id}/paradas")]
    [AllowAnonymous]
    public async Task<ActionResult> GetParadasDeViaje(int id)
    {
        try
        {
            var viaje = await _context.Viajes.FindAsync(id);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            var paradas = await _context.ParadasViaje
                .Where(p => p.ViajeID == id && p.EsActiva)
                .OrderBy(p => p.OrdenParada)
                .Select(p => new
                {
                    p.ParadaViajeID,
                    p.NombreParada,
                    Direccion = p.Direccion,
                    p.Latitud,
                    p.Longitud,
                    p.OrdenParada,
                    HoraEstimadaLlegada = p.HoraEstimadaLlegada,
                    EsActiva = p.EsActiva
                })
                .ToListAsync();

            return Ok(paradas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener paradas del viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al obtener paradas", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener manifiesto de pasajeros de un viaje
    /// Soporta parámetros opcionales:
    ///   compact=true  -> devuelve solo campos mínimos de cada pasajero
    ///   since=ISO8601 -> devuelve solo pasajeros cuyo estado cambió después de esa fecha
    /// </summary>
    [HttpGet("{id}/manifiesto")]
    [ClRequirePermission(ClAppPermissions.ViajesView)]
    public async Task<ActionResult<ManifiestoResponseDto>> GetManifiesto(int id, [FromQuery] bool compact = false, [FromQuery] DateTime? since = null)
    {
        try
        {
            // Códigos de estatus (evita números mágicos)
            const int ESTATUS_BOLETO_PAGADO = 10; // BOL_PAGADO
            const int ESTATUS_BOLETO_VALIDADO = 11; // BOL_VALIDADO
            const int ESTATUS_BOLETO_USADO = 12; // BOL_USADO
            const int ABD_ABORDADO = 22; // ABD_ABORDADO
            const int ABD_NO_PRESENTO = 23; // ABD_NO_PRESENTO

            var viaje = await _context.Viajes
                .Include(v => v.EstatusNavigation)
                .FirstOrDefaultAsync(v => v.ViajeID == id);
            
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            // Boletos válidos para abordaje (Pagado, Validado, Usado)
            var boletosQuery = _context.Boletos
                .Include(b => b.Cliente)
                .Include(b => b.ManifiestoPasajero)
                .Include(b => b.ParadaAbordaje)
                .Where(b => b.ViajeID == id && (b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_VALIDADO || b.Estatus == ESTATUS_BOLETO_USADO));

            // Filtro delta (since) basado en última acción relevante
            if (since.HasValue)
            {
                var s = since.Value.ToUniversalTime();
                boletosQuery = boletosQuery.Where(b =>
                    (b.FechaValidacion.HasValue && b.FechaValidacion.Value.ToUniversalTime() > s) ||
                    (b.ManifiestoPasajero != null && (
                        (b.ManifiestoPasajero.FechaValidacion.HasValue && b.ManifiestoPasajero.FechaValidacion.Value.ToUniversalTime() > s) ||
                        (b.ManifiestoPasajero.FechaAbordaje.HasValue && b.ManifiestoPasajero.FechaAbordaje.Value.ToUniversalTime() > s)))
                );
            }

            var boletos = await boletosQuery
                .OrderBy(b => b.NumeroAsiento)
                .ToListAsync();
            
            int totalPasajeros = await _context.Boletos
                .CountAsync(b => b.ViajeID == id && (b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_VALIDADO || b.Estatus == ESTATUS_BOLETO_USADO));

            int abordados = await _context.ManifiestoPasajeros
                .CountAsync(m => m.ViajeID == id && m.EstatusAbordaje == ABD_ABORDADO);
            
            int noAsistieron = await _context.ManifiestoPasajeros
                .CountAsync(m => m.ViajeID == id && m.EstatusAbordaje == ABD_NO_PRESENTO);
            
            int pendientes = totalPasajeros - abordados - noAsistieron;

            var pasajeros = boletos.Select(b => compact
                ? new PasajeroManifiestoDto
                {
                    BoletoID = b.BoletoID,
                    CodigoQR = b.CodigoQR,
                    ClienteID = b.ClienteID,
                    ClienteNombre = b.NombrePasajero ?? b.Cliente?.NombreCompleto ?? "Sin nombre",
                    AsientoAsignado = b.NumeroAsiento,
                    EstadoBoleto = b.Estatus switch
                    {
                        ESTATUS_BOLETO_PAGADO => "Pagado",
                        ESTATUS_BOLETO_VALIDADO => "Validado",
                        ESTATUS_BOLETO_USADO => "Usado",
                        _ => "Otro"
                    },
                    EstadoAbordaje = b.ManifiestoPasajero?.EstatusAbordaje switch
                    {
                        ABD_ABORDADO => "Abordado",
                        ABD_NO_PRESENTO => "No Presentó",
                        _ => "Pendiente"
                    },
                    FechaValidacion = b.FechaValidacion,
                    ValidadoPor = b.ManifiestoPasajero?.ValidadoPor
                }
                : new PasajeroManifiestoDto
                {
                    BoletoID = b.BoletoID,
                    CodigoQR = b.CodigoQR,
                    ClienteID = b.ClienteID,
                    ClienteNombre = b.NombrePasajero ?? b.Cliente?.NombreCompleto ?? "Sin nombre",
                    ClienteEmail = b.EmailPasajero ?? b.Cliente?.Email,
                    ClienteTelefono = b.TelefonoPasajero ?? b.Cliente?.PhoneNumber,
                    AsientoAsignado = b.NumeroAsiento,
                    EstadoBoleto = b.Estatus switch
                    {
                        ESTATUS_BOLETO_PAGADO => "Pagado",
                        ESTATUS_BOLETO_VALIDADO => "Validado",
                        ESTATUS_BOLETO_USADO => "Usado",
                        _ => "Otro"
                    },
                    EstadoAbordaje = b.ManifiestoPasajero?.EstatusAbordaje switch
                    {
                        ABD_ABORDADO => "Abordado",
                        ABD_NO_PRESENTO => "No Presentó",
                        _ => "Pendiente"
                    },
                    FechaValidacion = b.FechaValidacion,
                    ValidadoPor = b.ManifiestoPasajero?.ValidadoPor
                }).ToList();
            
            var response = new ManifiestoResponseDto
            {
                ViajeID = viaje.ViajeID,
                CodigoViaje = viaje.CodigoViaje,
                FechaSalida = viaje.FechaSalida,
                TotalPasajeros = totalPasajeros,
                PasajerosAbordados = abordados,
                PasajerosPendientes = pendientes,
                PasajerosNoAsistieron = noAsistieron,
                Pasajeros = pasajeros
            };
            
            _logger.LogInformation(
                "Manifiesto generado para viaje {ViajeID}: Total={Total} Devueltos={Devueltos} Compact={Compact} Since={Since}", 
                id, totalPasajeros, pasajeros.Count, compact, since);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener manifiesto del viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al obtener manifiesto", error = ex.Message });
        }
    }
    
    // NOTE: Las operaciones relacionadas con staff de un viaje (POST/GET/DELETE /api/viajes/{viajeId}/staff)
    // están implementadas de forma dedicada en `ViajeStaffController`. Para evitar rutas duplicadas y
    // conflictos de enrutamiento, no implementamos esas acciones aquí. Use `ViajeStaffController`.
}
