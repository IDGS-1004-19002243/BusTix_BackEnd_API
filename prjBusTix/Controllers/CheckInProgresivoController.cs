using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Viajes;
using prjBusTix.Model;
using prjBusTix.Security;
using prjBusTix.Services;
using System.Security.Claims;

namespace prjBusTix.Controllers;

/// <summary>
/// Controlador para gestionar el check-in progresivo en viajes
/// Flujo: Chofer confirma llegada → Staff valida pasajeros
/// </summary>
[ApiController]
[Route("api/viajes/{viajeId}/checkin")]
[Authorize]
public class CheckInProgresivoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly INotificacionService _notificacionService;
    private readonly ILogger<CheckInProgresivoController> _logger;

    public CheckInProgresivoController(
        AppDbContext context,
        INotificacionService notificacionService,
        ILogger<CheckInProgresivoController> logger)
    {
        _context = context;
        _notificacionService = notificacionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener el progreso completo de un viaje
    /// GET: api/viajes/{viajeId}/checkin/progreso
    /// </summary>
    [HttpGet("progreso")]
    [ProducesResponseType(typeof(ProgresoViajeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgresoViajeDto>> GetProgresoViaje(int viajeId)
    {
        try
        {
            var viaje = await _context.Viajes
                .Include(v => v.Paradas)
                .Include(v => v.ManifiestoPasajeros)
                .FirstOrDefaultAsync(v => v.ViajeID == viajeId);

            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            // Obtener estados de todas las paradas
            var estadosParadas = await _context.EstadosParadaViaje
                .Include(e => e.ParadaViaje)
                .Include(e => e.Chofer)
                .Include(e => e.Staff)
                .Where(e => e.ViajeID == viajeId)
                .OrderBy(e => e.ParadaViaje.OrdenParada)
                .Select(e => new EstadoParadaResponseDto
                {
                    EstadoParadaViajeID = e.EstadoParadaViajeID,
                    ViajeID = e.ViajeID,
                    CodigoViaje = viaje.CodigoViaje,
                    ParadaViajeID = e.ParadaViajeID,
                    NombreParada = e.ParadaViaje.NombreParada,
                    Direccion = e.ParadaViaje.Direccion ?? "",
                    OrdenParada = e.ParadaViaje.OrdenParada,
                    HoraEstimadaLlegada = e.ParadaViaje.HoraEstimadaLlegada,
                    Estado = e.Estado,
                    FechaHoraLlegadaChofer = e.FechaHoraLlegadaChofer,
                    ChoferID = e.ConfirmadoPorChofer,
                    NombreChofer = e.Chofer != null ? e.Chofer.NombreCompleto : null,
                    LatitudConfirmacion = e.LatitudConfirmacion,
                    LongitudConfirmacion = e.LongitudConfirmacion,
                    FechaHoraInicioValidacion = e.FechaHoraInicioValidacion,
                    FechaHoraFinalizacionValidacion = e.FechaHoraFinalizacionValidacion,
                    StaffID = e.ValidadoPorStaff,
                    NombreStaff = e.Staff != null ? e.Staff.NombreCompleto : null,
                    TotalPasajerosEsperados = e.TotalPasajerosEsperados,
                    TotalPasajerosAbordados = e.TotalPasajerosAbordados,
                    TotalPasajerosNoShow = e.TotalPasajerosNoShow,
                    TuvoIncidencia = e.TuvoIncidencia,
                    Observaciones = e.Observaciones
                })
                .ToListAsync();

            // Si no hay estados, crearlos automáticamente
            if (!estadosParadas.Any())
            {
                await InicializarEstadosParadas(viajeId);
                // Volver a consultar después de inicializar
                return await GetProgresoViaje(viajeId);
            }

            var paradasCompletadas = estadosParadas.Count(e => e.Estado == "Completado");
            var paradaActual = estadosParadas.FirstOrDefault(e => e.Estado != "Completado")?.OrdenParada ?? viaje.Paradas.Count;

            var progreso = new ProgresoViajeDto
            {
                ViajeID = viajeId,
                CodigoViaje = viaje.CodigoViaje,
                EstadoGeneral = DeterminarEstadoGeneral(estadosParadas),
                FechaSalida = viaje.FechaSalida,
                TotalParadas = viaje.Paradas.Count,
                ParadasCompletadas = paradasCompletadas,
                ParadaActual = paradaActual,
                Paradas = estadosParadas,
                TotalPasajerosViaje = viaje.CupoTotal,
                TotalAbordados = estadosParadas.Sum(e => e.TotalPasajerosAbordados),
                TotalNoShow = estadosParadas.Sum(e => e.TotalPasajerosNoShow)
            };

            return Ok(progreso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener progreso del viaje {ViajeId}", viajeId);
            return StatusCode(500, new { message = "Error al obtener el progreso del viaje" });
        }
    }

    /// <summary>
    /// Chofer confirma llegada a una parada
    /// POST: api/viajes/{viajeId}/checkin/confirmar-llegada
    /// </summary>
    [HttpPost("confirmar-llegada")]
    [ClRequirePermission(AppPermissions.Viajes.Validate)]
    [ProducesResponseType(typeof(EstadoParadaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EstadoParadaResponseDto>> ConfirmarLlegada(
        int viajeId, 
        [FromBody] ConfirmarLlegadaParadaDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });

            // Verificar que el usuario es el chofer del viaje
            var viaje = await _context.Viajes
                .Include(v => v.Chofer)
                .FirstOrDefaultAsync(v => v.ViajeID == viajeId);

            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            if (viaje.ChoferID != userId)
                return BadRequest(new { message = "Solo el chofer asignado puede confirmar llegadas" });

            // Buscar el estado de la parada
            var estadoParada = await _context.EstadosParadaViaje
                .Include(e => e.ParadaViaje)
                .FirstOrDefaultAsync(e => e.ViajeID == viajeId && e.ParadaViajeID == dto.ParadaViajeID);

            if (estadoParada == null)
            {
                // Inicializar estados si no existen
                await InicializarEstadosParadas(viajeId);
                estadoParada = await _context.EstadosParadaViaje
                    .Include(e => e.ParadaViaje)
                    .FirstOrDefaultAsync(e => e.ViajeID == viajeId && e.ParadaViajeID == dto.ParadaViajeID);
                
                if (estadoParada == null)
                    return NotFound(new { message = "Parada no encontrada" });
            }

            // Validar que la parada está en estado adecuado
            if (estadoParada.Estado == "Completado")
                return BadRequest(new { message = "Esta parada ya ha sido completada" });

            // Actualizar estado
            estadoParada.Estado = "Llegado";
            estadoParada.FechaHoraLlegadaChofer = DateTime.Now;
            estadoParada.ConfirmadoPorChofer = userId;
            estadoParada.LatitudConfirmacion = dto.Latitud;
            estadoParada.LongitudConfirmacion = dto.Longitud;
            
            if (!string.IsNullOrEmpty(dto.Observaciones))
            {
                estadoParada.Observaciones = (estadoParada.Observaciones ?? "") + 
                    $"\n[Chofer {DateTime.Now:yyyy-MM-dd HH:mm}]: {dto.Observaciones}";
            }

            await _context.SaveChangesAsync();

            // Notificar a los pasajeros que abordan en esta parada
            await _notificacionService.NotificarLlegadaChoferParadaAsync(viajeId, dto.ParadaViajeID);

            // Notificar al staff asignado
            var staffAsignado = await _context.ViajesStaff
                .Where(vs => vs.ViajeID == viajeId)
                .Select(vs => vs.StaffID)
                .ToListAsync();

            foreach (var staffId in staffAsignado)
            {
                var notificacion = new Notificacion
                {
                    UsuarioID = staffId,
                    Titulo = "Llegada a parada confirmada",
                    Mensaje = $"El chofer ha llegado a {estadoParada.ParadaViaje.NombreParada}. Puedes iniciar validación de pasajeros.",
                    TipoNotificacion = "checkin_parada",
                    FechaCreacion = DateTime.Now,
                    FueLeida = false,
                    ViajeID = viajeId
                };
                
                await _notificacionService.EnviarNotificacionAsync(notificacion);
            }

            await transaction.CommitAsync();

            var response = await MapearEstadoParada(estadoParada);
            return Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al confirmar llegada a parada {ParadaId} del viaje {ViajeId}", 
                dto.ParadaViajeID, viajeId);
            return StatusCode(500, new { message = "Error al confirmar la llegada" });
        }
    }

    /// <summary>
    /// Staff inicia validación de pasajeros en una parada
    /// POST: api/viajes/{viajeId}/checkin/iniciar-validacion
    /// </summary>
    [HttpPost("iniciar-validacion")]
    [ClRequirePermission(AppPermissions.Viajes.Validate)]
    [ProducesResponseType(typeof(EstadoParadaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EstadoParadaResponseDto>> IniciarValidacion(
        int viajeId,
        [FromBody] IniciarValidacionParadaDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });

            // Verificar que el usuario es staff del viaje
            var esStaff = await _context.ViajesStaff
                .AnyAsync(vs => vs.ViajeID == viajeId && vs.StaffID == userId);

            if (!esStaff)
                return BadRequest(new { message = "Solo el staff asignado puede iniciar validación" });

            var estadoParada = await _context.EstadosParadaViaje
                .Include(e => e.ParadaViaje)
                .FirstOrDefaultAsync(e => e.ViajeID == viajeId && e.ParadaViajeID == dto.ParadaViajeID);

            if (estadoParada == null)
                return NotFound(new { message = "Estado de parada no encontrado" });

            // Validar que el chofer ya llegó
            if (estadoParada.Estado != "Llegado" && estadoParada.Estado != "Validando")
                return BadRequest(new { message = "El chofer aún no ha confirmado llegada a esta parada" });

            // Actualizar estado
            estadoParada.Estado = "Validando";
            estadoParada.FechaHoraInicioValidacion = DateTime.Now;
            estadoParada.ValidadoPorStaff = userId;
            
            if (!string.IsNullOrEmpty(dto.Observaciones))
            {
                estadoParada.Observaciones = (estadoParada.Observaciones ?? "") + 
                    $"\n[Staff {DateTime.Now:yyyy-MM-dd HH:mm}]: {dto.Observaciones}";
            }

            await _context.SaveChangesAsync();

            var response = await MapearEstadoParada(estadoParada);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar validación en parada {ParadaId} del viaje {ViajeId}", 
                dto.ParadaViajeID, viajeId);
            return StatusCode(500, new { message = "Error al iniciar la validación" });
        }
    }

    /// <summary>
    /// Staff finaliza validación de pasajeros en una parada
    /// POST: api/viajes/{viajeId}/checkin/finalizar-validacion
    /// </summary>
    [HttpPost("finalizar-validacion")]
    [ClRequirePermission(AppPermissions.Viajes.Validate)]
    [ProducesResponseType(typeof(EstadoParadaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EstadoParadaResponseDto>> FinalizarValidacion(
        int viajeId,
        [FromBody] FinalizarValidacionParadaDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });

            var estadoParada = await _context.EstadosParadaViaje
                .Include(e => e.ParadaViaje)
                .FirstOrDefaultAsync(e => e.ViajeID == viajeId && e.ParadaViajeID == dto.ParadaViajeID);

            if (estadoParada == null)
                return NotFound(new { message = "Estado de parada no encontrado" });

            if (estadoParada.Estado != "Validando")
                return BadRequest(new { message = "La validación no ha sido iniciada" });

            // Actualizar estado
            estadoParada.Estado = "Completado";
            estadoParada.FechaHoraFinalizacionValidacion = DateTime.Now;
            estadoParada.TotalPasajerosAbordados = dto.TotalAbordados;
            estadoParada.TotalPasajerosNoShow = dto.TotalNoShow;
            
            if (!string.IsNullOrEmpty(dto.Observaciones))
            {
                estadoParada.Observaciones = (estadoParada.Observaciones ?? "") + 
                    $"\n[Finalización {DateTime.Now:yyyy-MM-dd HH:mm}]: {dto.Observaciones}";
            }

            await _context.SaveChangesAsync();

            // Notificar al chofer
            var viaje = await _context.Viajes.FindAsync(viajeId);
            if (viaje?.ChoferID != null)
            {
                var notificacion = new Notificacion
                {
                    UsuarioID = viaje.ChoferID,
                    Titulo = "Validación completada",
                    Mensaje = $"Se completó la validación en {estadoParada.ParadaViaje.NombreParada}. " +
                             $"Abordaron: {dto.TotalAbordados}, No Show: {dto.TotalNoShow}",
                    TipoNotificacion = "checkin_completado",
                    FechaCreacion = DateTime.Now,
                    FueLeida = false,
                    ViajeID = viajeId
                };
                
                await _notificacionService.EnviarNotificacionAsync(notificacion);
            }

            await transaction.CommitAsync();

            var response = await MapearEstadoParada(estadoParada);
            return Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al finalizar validación en parada {ParadaId} del viaje {ViajeId}", 
                dto.ParadaViajeID, viajeId);
            return StatusCode(500, new { message = "Error al finalizar la validación" });
        }
    }

    #region Métodos Auxiliares

    /// <summary>
    /// Inicializa los estados de todas las paradas de un viaje
    /// </summary>
    private async Task InicializarEstadosParadas(int viajeId)
    {
        var paradas = await _context.ParadasViaje
            .Where(p => p.ViajeID == viajeId && p.EsActiva)
            .OrderBy(p => p.OrdenParada)
            .ToListAsync();

        foreach (var parada in paradas)
        {
            var estadoExiste = await _context.EstadosParadaViaje
                .AnyAsync(e => e.ViajeID == viajeId && e.ParadaViajeID == parada.ParadaViajeID);

            if (!estadoExiste)
            {
                // Contar pasajeros esperados en esta parada
                var pasajerosEsperados = await _context.Boletos
                    .Where(b => b.ViajeID == viajeId && 
                               b.ParadaAbordajeID == parada.ParadaViajeID &&
                               b.Estatus == 10) // BOL_PAGADO
                    .CountAsync();

                var nuevoEstado = new EstadoParadaViaje
                {
                    ViajeID = viajeId,
                    ParadaViajeID = parada.ParadaViajeID,
                    Estado = "Pendiente",
                    TotalPasajerosEsperados = pasajerosEsperados,
                    TotalPasajerosAbordados = 0,
                    TotalPasajerosNoShow = 0
                };

                _context.EstadosParadaViaje.Add(nuevoEstado);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Determina el estado general del viaje basado en el progreso de las paradas
    /// </summary>
    private string DeterminarEstadoGeneral(List<EstadoParadaResponseDto> paradas)
    {
        if (!paradas.Any())
            return "Pendiente";

        var todasCompletadas = paradas.All(p => p.Estado == "Completado");
        if (todasCompletadas)
            return "Completado";

        var algunaEnProceso = paradas.Any(p => p.Estado == "Llegado" || p.Estado == "Validando");
        if (algunaEnProceso)
            return "EnRuta";

        return "Pendiente";
    }

    /// <summary>
    /// Mapea un EstadoParadaViaje a su DTO de respuesta
    /// </summary>
    private async Task<EstadoParadaResponseDto> MapearEstadoParada(EstadoParadaViaje estado)
    {
        var viaje = await _context.Viajes.FindAsync(estado.ViajeID);
        var chofer = estado.ConfirmadoPorChofer != null 
            ? await _context.Users.FindAsync(estado.ConfirmadoPorChofer) 
            : null;
        var staff = estado.ValidadoPorStaff != null 
            ? await _context.Users.FindAsync(estado.ValidadoPorStaff) 
            : null;

        return new EstadoParadaResponseDto
        {
            EstadoParadaViajeID = estado.EstadoParadaViajeID,
            ViajeID = estado.ViajeID,
            CodigoViaje = viaje?.CodigoViaje ?? "",
            ParadaViajeID = estado.ParadaViajeID,
            NombreParada = estado.ParadaViaje.NombreParada,
            Direccion = estado.ParadaViaje.Direccion ?? "",
            OrdenParada = estado.ParadaViaje.OrdenParada,
            HoraEstimadaLlegada = estado.ParadaViaje.HoraEstimadaLlegada,
            Estado = estado.Estado,
            FechaHoraLlegadaChofer = estado.FechaHoraLlegadaChofer,
            ChoferID = estado.ConfirmadoPorChofer,
            NombreChofer = chofer?.NombreCompleto,
            LatitudConfirmacion = estado.LatitudConfirmacion,
            LongitudConfirmacion = estado.LongitudConfirmacion,
            FechaHoraInicioValidacion = estado.FechaHoraInicioValidacion,
            FechaHoraFinalizacionValidacion = estado.FechaHoraFinalizacionValidacion,
            StaffID = estado.ValidadoPorStaff,
            NombreStaff = staff?.NombreCompleto,
            TotalPasajerosEsperados = estado.TotalPasajerosEsperados,
            TotalPasajerosAbordados = estado.TotalPasajerosAbordados,
            TotalPasajerosNoShow = estado.TotalPasajerosNoShow,
            TuvoIncidencia = estado.TuvoIncidencia,
            Observaciones = estado.Observaciones
        };
    }

    #endregion
}

