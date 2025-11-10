using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Viajes;
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
    /// Crear un nuevo viaje
    /// </summary>
    [HttpPost]
    [ClRequirePermission(ClAppPermissions.ViajesCreate)]
    public async Task<ActionResult<ViajeResponseDto>> CrearViaje([FromBody] CrearViajeDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validar que el evento existe
            var evento = await _context.Eventos.FindAsync(dto.EventoID);
            if (evento == null)
                return BadRequest(new { message = "El evento especificado no existe" });

            // Validar que la plantilla de ruta existe
            var plantillaRuta = await _context.PlantillasRutas.FindAsync(dto.PlantillaRutaID);
            if (plantillaRuta == null || !plantillaRuta.Activa)
                return BadRequest(new { message = "La plantilla de ruta especificada no existe o no est� activa" });

            // Validar unidad si se proporciona
            if (dto.UnidadID.HasValue)
            {
                var unidad = await _context.Unidades.FindAsync(dto.UnidadID.Value);
                if (unidad == null || unidad.Estatus != 1)
                    return BadRequest(new { message = "La unidad especificada no existe o no est� disponible" });
            }

            // Validar chofer si se proporciona
            if (!string.IsNullOrEmpty(dto.ChoferID))
            {
                var chofer = await _context.Users.FindAsync(dto.ChoferID);
                if (chofer == null || chofer.Estatus != 1)
                    return BadRequest(new { message = "El chofer especificado no existe o no est� activo" });
            }

            // Generar c�digo �nico de viaje
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
            var viaje = await _context.Viajes.FindAsync(id);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            // No permitir actualizar viajes ya iniciados o terminados
            if (viaje.Estatus > 2)
                return BadRequest(new { message = "No se puede modificar un viaje que ya inici� o termin�" });

            // Validar unidad si se proporciona
            if (dto.UnidadID.HasValue)
            {
                var unidad = await _context.Unidades.FindAsync(dto.UnidadID.Value);
                if (unidad == null || unidad.Estatus != 1)
                    return BadRequest(new { message = "La unidad especificada no existe o no est� disponible" });
                viaje.UnidadID = dto.UnidadID;
            }

            // Validar chofer si se proporciona
            if (dto.ChoferID != null)
            {
                if (!string.IsNullOrEmpty(dto.ChoferID))
                {
                    var chofer = await _context.Users.FindAsync(dto.ChoferID);
                    if (chofer == null || chofer.Estatus != 1)
                        return BadRequest(new { message = "El chofer especificado no existe o no est� activo" });
                }
                viaje.ChoferID = dto.ChoferID;
            }

            if (dto.FechaSalida.HasValue)
                viaje.FechaSalida = dto.FechaSalida.Value;

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
    /// </summary>
    [HttpGet("{id}/manifiesto")]
    [ClRequirePermission(ClAppPermissions.ViajesView)]
    public async Task<ActionResult> GetManifiestoViaje(int id)
    {
        try
        {
            var viaje = await _context.Viajes.FindAsync(id);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            var manifiesto = await _context.ManifiestoPasajeros
                .Include(m => m.Boleto)
                    .ThenInclude(b => b.Cliente)
                .Include(m => m.Boleto)
                    .ThenInclude(b => b.ParadaAbordaje)
                .Where(m => m.ViajeID == id)
                .OrderBy(m => m.Boleto.NumeroAsiento)
                .Select(m => new
                {
                    m.ManifiestoID,
                    m.BoletoID,
                    m.Boleto.CodigoBoleto,
                    m.Boleto.NombrePasajero,
                    m.Boleto.EmailPasajero,
                    m.Boleto.TelefonoPasajero,
                    m.Boleto.NumeroAsiento,
                    ParadaAbordaje = m.Boleto.ParadaAbordaje != null ? m.Boleto.ParadaAbordaje.NombreParada : null,
                    EstatusAbordaje = m.EstatusAbordaje,
                    FechaAbordaje = m.FechaAbordaje,
                    FueValidado = m.FueValidado
                })
                .ToListAsync();

            return Ok(manifiesto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener manifiesto del viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al obtener manifiesto", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Asignar staff a un viaje
    /// POST /api/viajes/{id}/staff
    /// </summary>
    [HttpPost("{id}/staff")]
    [ClRequirePermission(ClAppPermissions.ViajesUpdate)]
    public async Task<ActionResult<StaffViajeResponseDto>> AsignarStaff(int id, [FromBody] AsignarStaffDto dto)
    {
        try
        {
            var viaje = await _context.Viajes.FindAsync(id);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });
            
            // Validar que el viaje no haya salido
            if (viaje.FechaSalida < DateTime.Now)
                return BadRequest(new { message = "No se puede asignar staff a un viaje que ya salió" });
            
            // Validar que el staff existe y está activo
            var staff = await _context.Users.FindAsync(dto.StaffID);
            if (staff == null || staff.Estatus != 1)
                return BadRequest(new { message = "El staff especificado no existe o no está activo" });
            
            // Verificar que el staff no esté ya asignado al mismo viaje
            var yaAsignado = await _context.ViajesStaff
                .AnyAsync(vs => vs.ViajeID == id && vs.StaffID == dto.StaffID);
            
            if (yaAsignado)
                return BadRequest(new { message = "Este staff ya está asignado a este viaje" });
            
            // Crear asignación
            var asignacion = new ViajeStaff
            {
                ViajeID = id,
                StaffID = dto.StaffID,
                RolEnViaje = dto.RolEnViaje,
                FechaAsignacion = DateTime.Now,
                Observaciones = dto.Observaciones
            };
            
            _context.ViajesStaff.Add(asignacion);
            await _context.SaveChangesAsync();
            
            // Preparar respuesta
            var response = new StaffViajeResponseDto
            {
                AsignacionID = asignacion.AsignacionID,
                ViajeID = asignacion.ViajeID,
                StaffID = asignacion.StaffID,
                StaffNombre = staff.NombreCompleto,
                StaffEmail = staff.Email,
                StaffTelefono = staff.PhoneNumber,
                RolEnViaje = asignacion.RolEnViaje,
                FechaAsignacion = asignacion.FechaAsignacion,
                Observaciones = asignacion.Observaciones
            };
            
            _logger.LogInformation("Staff {StaffID} asignado al viaje {ViajeID} como {Rol}", 
                dto.StaffID, id, dto.RolEnViaje);
            
            return CreatedAtAction(nameof(GetStaffDeViaje), new { id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar staff al viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al asignar staff", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtener staff asignado a un viaje
    /// GET /api/viajes/{id}/staff
    /// </summary>
    [HttpGet("{id}/staff")]
    [ClRequirePermission(ClAppPermissions.ViajesView)]
    public async Task<ActionResult<IEnumerable<StaffViajeResponseDto>>> GetStaffDeViaje(int id)
    {
        try
        {
            var viaje = await _context.Viajes.FindAsync(id);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });
            
            var staff = await _context.ViajesStaff
                .Include(vs => vs.Staff)
                .Where(vs => vs.ViajeID == id)
                .OrderBy(vs => vs.FechaAsignacion)
                .Select(vs => new StaffViajeResponseDto
                {
                    AsignacionID = vs.AsignacionID,
                    ViajeID = vs.ViajeID,
                    StaffID = vs.StaffID,
                    StaffNombre = vs.Staff.NombreCompleto,
                    StaffEmail = vs.Staff.Email,
                    StaffTelefono = vs.Staff.PhoneNumber,
                    RolEnViaje = vs.RolEnViaje,
                    FechaAsignacion = vs.FechaAsignacion,
                    Observaciones = vs.Observaciones
                })
                .ToListAsync();
            
            return Ok(staff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener staff del viaje {ViajeID}", id);
            return StatusCode(500, new { message = "Error al obtener staff", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Desasignar staff de un viaje
    /// DELETE /api/viajes/{viajeId}/staff/{asignacionId}
    /// </summary>
    [HttpDelete("{viajeId}/staff/{asignacionId}")]
    [ClRequirePermission(ClAppPermissions.ViajesUpdate)]
    public async Task<ActionResult> DesasignarStaff(int viajeId, int asignacionId)
    {
        try
        {
            var asignacion = await _context.ViajesStaff
                .FirstOrDefaultAsync(vs => vs.AsignacionID == asignacionId && vs.ViajeID == viajeId);
            
            if (asignacion == null)
                return NotFound(new { message = "Asignación no encontrada" });
            
            // Validar que el viaje no haya salido
            var viaje = await _context.Viajes.FindAsync(viajeId);
            if (viaje != null && viaje.FechaSalida < DateTime.Now)
                return BadRequest(new { message = "No se puede desasignar staff de un viaje que ya salió" });
            
            _context.ViajesStaff.Remove(asignacion);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Staff desasignado del viaje {ViajeID}: AsignacionID {AsignacionID}", 
                viajeId, asignacionId);
            
            return Ok(new { message = "Staff desasignado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desasignar staff del viaje {ViajeID}", viajeId);
            return StatusCode(500, new { message = "Error al desasignar staff", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtener viajes asignados a un staff
    /// GET /api/viajes/mis-viajes
    /// </summary>
    [HttpGet("mis-viajes")]
    [Authorize(Roles = "Staff,Chofer,Manager")]
    public async Task<ActionResult> GetMisViajes(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] bool? soloProximos = true)
    {
        try
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var query = _context.ViajesStaff
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.Evento)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.Unidad)
                .Include(vs => vs.Viaje)
                    .ThenInclude(v => v.EstatusNavigation)
                .Where(vs => vs.StaffID == staffId)
                .AsQueryable();
            
            if (fechaDesde.HasValue)
                query = query.Where(vs => vs.Viaje.FechaSalida >= fechaDesde.Value);
            
            if (fechaHasta.HasValue)
                query = query.Where(vs => vs.Viaje.FechaSalida <= fechaHasta.Value);
            
            if (soloProximos == true)
                query = query.Where(vs => vs.Viaje.FechaSalida >= DateTime.Now);
            
            var viajes = await query
                .OrderBy(vs => vs.Viaje.FechaSalida)
                .Select(vs => new
                {
                    asignacionID = vs.AsignacionID,
                    rolEnViaje = vs.RolEnViaje,
                    fechaAsignacion = vs.FechaAsignacion,
                    viaje = new
                    {
                        vs.Viaje.ViajeID,
                        vs.Viaje.CodigoViaje,
                        vs.Viaje.TipoViaje,
                        eventoNombre = vs.Viaje.Evento.Nombre,
                        rutaNombre = vs.Viaje.PlantillaRuta.NombreRuta,
                        ciudadOrigen = vs.Viaje.PlantillaRuta.CiudadOrigen,
                        ciudadDestino = vs.Viaje.PlantillaRuta.CiudadDestino,
                        vs.Viaje.FechaSalida,
                        vs.Viaje.FechaLlegadaEstimada,
                        unidadPlacas = vs.Viaje.Unidad != null ? vs.Viaje.Unidad.Placas : null,
                        vs.Viaje.CupoTotal,
                        vs.Viaje.AsientosVendidos,
                        vs.Viaje.AsientosDisponibles,
                        estatus = vs.Viaje.EstatusNavigation.Nombre
                    }
                })
                .ToListAsync();
            
            return Ok(new
            {
                success = true,
                data = viajes,
                total = viajes.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener viajes del staff");
            return StatusCode(500, new { message = "Error al obtener viajes", error = ex.Message });
        }
    }
}

