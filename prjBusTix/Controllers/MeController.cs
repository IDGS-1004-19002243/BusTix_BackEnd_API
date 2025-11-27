﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using prjBusTix.Model;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Boletos;
using prjBusTix.Dto.Auth;
using System.Security.Claims;

namespace prjBusTix.Controllers;

/// <summary>
/// Controlador para recursos del usuario autenticado (/me)
/// </summary>
[ApiController]
[Route("api/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<MeController> _logger;
    private readonly UserManager<ClApplicationUser> _userManager;

    public MeController(
        AppDbContext context, 
        ILogger<MeController> logger,
        UserManager<ClApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    /// <summary>
    /// Obtiene el historial de boletos del usuario autenticado
    /// GET /api/me/boletos
    /// </summary>
    [HttpGet("boletos")]
    public async Task<ActionResult<IEnumerable<EventoBoletosDto>>> GetMisBoletos(
        [FromQuery] string? estatus = null,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { 
                    success = false,
                    message = "Usuario no autenticado" 
                });

            var query = _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.Evento)
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.Unidad)
                .Include(b => b.ParadaAbordaje)
                .Include(b => b.EstatusNavigation)
                .Include(b => b.PagosBoletos)
                    .ThenInclude(pb => pb.Pago)
                .Where(b => b.ClienteID == userId)
                .AsQueryable();

            // Filtrar por estatus si se proporciona
            if (!string.IsNullOrWhiteSpace(estatus))
            {
                query = query.Where(b => b.EstatusNavigation.Codigo == estatus);
            }

            // Filtrar solo boletos activos (próximos viajes)
            if (soloActivos == true)
            {
                const int ESTATUS_BOLETO_PAGADO = 10;
                query = query.Where(b => 
                    b.Estatus == ESTATUS_BOLETO_PAGADO && 
                    b.Viaje.FechaSalida > DateTime.Now);
            }

            var boletos = await query
                .OrderByDescending(b => b.FechaCompra)
                .ToListAsync();

            // Agrupar por Evento
            var eventosGroup = boletos
                .GroupBy(b => b.Viaje.Evento)
                .Select(gEvento => new EventoBoletosDto
                {
                    EventoID = gEvento.Key.EventoID,
                    NombreEvento = gEvento.Key.Nombre,
                    ImagenEvento = gEvento.Key.UrlImagen,
                    UbicacionEvento = string.Join(", ", new[] { gEvento.Key.Recinto, gEvento.Key.Ciudad }.Where(s => !string.IsNullOrEmpty(s))),
                    FechaEvento = gEvento.Key.Fecha,
                    Transacciones = gEvento
                        // Agrupar por Transacción (Pago)
                        // Un boleto puede tener múltiples pagos, pero asumimos que para la vista de usuario
                        // queremos agrupar por el pago principal o el primero encontrado si hay varios.
                        // O mejor, agrupamos por el ID del Pago asociado al boleto.
                        // Nota: Un boleto podría no tener pago si es cortesía o 100% descuento, manejar ese caso.
                        .GroupBy(b => b.PagosBoletos.FirstOrDefault()?.Pago)
                        .Select(gPago => new TransaccionBoletosDto
                        {
                            PagoID = gPago.Key?.PagoID ?? 0,
                            CodigoPago = gPago.Key?.CodigoPago ?? "SIN-PAGO",
                            TransaccionID = gPago.Key?.TransaccionID,
                            FechaPago = gPago.Key?.FechaPago ?? gPago.First().FechaCompra,
                            MontoTotal = gPago.Key?.Monto ?? 0, // Monto total de la transacción
                            MetodoPago = gPago.Key?.MetodoPago ?? "N/A",
                            Boletos = gPago.Select(b => new BoletoCompletoDto
                            {
                                BoletoID = b.BoletoID,
                                CodigoBoleto = b.CodigoBoleto,
                                CodigoQR = b.CodigoQR,
                                NumeroAsiento = b.NumeroAsiento,
                                NombrePasajero = b.NombrePasajero,
                                PrecioTotal = b.PrecioTotal,
                                Estatus = b.Estatus,
                                EstatusNombre = b.EstatusNavigation.Nombre,
                                ParadaAbordaje = b.ParadaAbordaje?.NombreParada,
                                ParadaAbordajeLatitud = b.ParadaAbordaje?.Latitud,
                                ParadaAbordajeLongitud = b.ParadaAbordaje?.Longitud,
                                DetalleViaje = new BoletoDetalleViajeDto
                                {
                                    ViajeID = b.ViajeID,
                                    CodigoViaje = b.Viaje.CodigoViaje,
                                    CiudadOrigen = b.Viaje.PlantillaRuta.CiudadOrigen,
                                    CiudadDestino = b.Viaje.PlantillaRuta.CiudadDestino,
                                    FechaSalida = b.Viaje.FechaSalida,
                                    FechaLlegadaEstimada = b.Viaje.FechaLlegadaEstimada,
                                    UnidadPlacas = b.Viaje.Unidad?.Placas,
                                    UnidadNumeroEconomico = b.Viaje.Unidad?.NumeroEconomico
                                }
                            }).ToList()
                        }).ToList()
                }).ToList();

            return Ok(new
            {
                success = true,
                data = eventosGroup,
                total = eventosGroup.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener boletos del usuario");
            return StatusCode(500, new { 
                success = false,
                message = "Error al obtener los boletos" 
            });
        }
    }

    /// <summary>
    /// Obtiene información del perfil del usuario autenticado
    /// GET /api/me/perfil
    /// </summary>
    [HttpGet("perfil")]
    public async Task<ActionResult> GetMiPerfil()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var usuario = await _context.Users
                .Include(u => u.EstatusNavigation)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                return NotFound(new { 
                    success = false,
                    message = "Usuario no encontrado" 
                });

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = usuario.Id,
                    nombreCompleto = usuario.NombreCompleto,
                    email = usuario.Email,
                    telefono = usuario.PhoneNumber,
                    tipoDocumento = usuario.TipoDocumento,
                    numeroDocumento = usuario.NumeroDocumento,
                    fechaNacimiento = usuario.FechaNacimiento,
                    direccion = usuario.Direccion,
                    ciudad = usuario.Ciudad,
                    estado = usuario.Estado,
                    codigoPostal = usuario.CodigoPostal,
                    urlFotoPerfil = usuario.UrlFotoPerfil,
                    notificacionesPush = usuario.NotificacionesPush,
                    notificacionesEmail = usuario.NotificacionesEmail,
                    estatus = usuario.EstatusNavigation?.Nombre ?? "Desconocido",
                    fechaRegistro = usuario.FechaRegistro,
                    ultimaConexion = usuario.UltimaConexion
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil del usuario");
            return StatusCode(500, new { 
                success = false,
                message = "Error al obtener el perfil" 
            });
        }
    }

    /// <summary>
    /// Obtiene estadísticas del usuario (boletos comprados, viajes realizados, etc.)
    /// GET /api/me/estadisticas
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<ActionResult> GetMisEstadisticas()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;

            var totalBoletos = await _context.Boletos
                .CountAsync(b => b.ClienteID == userId);

            var boletosActivos = await _context.Boletos
                .CountAsync(b => b.ClienteID == userId && 
                               b.Estatus == ESTATUS_BOLETO_PAGADO &&
                               b.Viaje.FechaSalida > DateTime.Now);

            var viajesRealizados = await _context.Boletos
                .CountAsync(b => b.ClienteID == userId && 
                               b.Estatus == ESTATUS_BOLETO_USADO);

            var totalGastado = await _context.Boletos
                .Where(b => b.ClienteID == userId && 
                          (b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO))
                .SumAsync(b => b.PrecioTotal);

            var ultimoViaje = await _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Where(b => b.ClienteID == userId && b.Estatus == ESTATUS_BOLETO_USADO)
                .OrderByDescending(b => b.Viaje.FechaSalida)
                .Select(b => new {
                    origen = b.Viaje.PlantillaRuta.CiudadOrigen,
                    destino = b.Viaje.PlantillaRuta.CiudadDestino,
                    fecha = b.Viaje.FechaSalida
                })
                .FirstOrDefaultAsync();

            var proximoViaje = await _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Where(b => b.ClienteID == userId && 
                          b.Estatus == ESTATUS_BOLETO_PAGADO &&
                          b.Viaje.FechaSalida > DateTime.Now)
                .OrderBy(b => b.Viaje.FechaSalida)
                .Select(b => new {
                    origen = b.Viaje.PlantillaRuta.CiudadOrigen,
                    destino = b.Viaje.PlantillaRuta.CiudadDestino,
                    fecha = b.Viaje.FechaSalida,
                    codigoBoleto = b.CodigoBoleto
                })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalBoletos,
                    boletosActivos,
                    viajesRealizados,
                    totalGastado = Math.Round(totalGastado, 2),
                    ultimoViaje,
                    proximoViaje
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas del usuario");
            return StatusCode(500, new { 
                success = false,
                message = "Error al obtener las estadísticas" 
            });
        }
    }
    
    /// <summary>
    /// Obtiene el historial de notificaciones del usuario autenticado
    /// GET /api/me/notificaciones
    /// </summary>
    [HttpGet("notificaciones")]
    public async Task<ActionResult> GetMisNotificaciones(
        [FromQuery] bool? soloNoLeidas = null,
        [FromQuery] string? tipo = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { 
                    success = false,
                    message = "Usuario no autenticado" 
                });

            var query = _context.Notificaciones
                .Include(n => n.Viaje)
                    .ThenInclude(v => v!.PlantillaRuta)
                .Include(n => n.Boleto)
                .Where(n => n.UsuarioID == userId)
                .AsQueryable();

            // Filtrar por no leídas
            if (soloNoLeidas == true)
            {
                query = query.Where(n => !n.FueLeida);
            }

            // Filtrar por tipo
            if (!string.IsNullOrWhiteSpace(tipo))
            {
                query = query.Where(n => n.TipoNotificacion == tipo);
            }

            var total = await query.CountAsync();

            var notificaciones = await query
                .OrderByDescending(n => n.FechaCreacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(n => new
                {
                    notificacionID = n.NotificacionID,
                    titulo = n.Titulo,
                    mensaje = n.Mensaje,
                    tipoNotificacion = n.TipoNotificacion ?? "General",
                    fechaCreacion = n.FechaCreacion,
                    fueLeida = n.FueLeida,
                    fechaLectura = n.FechaLectura,
                    viajeID = n.ViajeID,
                    viaje = n.Viaje != null ? new
                    {
                        codigo = n.Viaje.CodigoViaje,
                        origen = n.Viaje.PlantillaRuta.CiudadOrigen,
                        destino = n.Viaje.PlantillaRuta.CiudadDestino,
                        fechaSalida = n.Viaje.FechaSalida
                    } : null,
                    boletoID = n.BoletoID,
                    boleto = n.Boleto != null ? new
                    {
                        codigo = n.Boleto.CodigoBoleto,
                        asiento = n.Boleto.NumeroAsiento
                    } : null
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = notificaciones,
                pagination = new
                {
                    total,
                    pagina,
                    tamanoPagina,
                    totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones del usuario");
            return StatusCode(500, new { 
                success = false,
                message = "Error al obtener las notificaciones" 
            });
        }
    }

    /// <summary>
    /// Actualiza la información del perfil del usuario
    /// PUT /api/me/perfil
    /// </summary>
    [HttpPut("perfil")]
    public async Task<ActionResult> ActualizarPerfil([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado" });

            // Actualizar campos
            usuario.NombreCompleto = dto.NombreCompleto;
            usuario.PhoneNumber = dto.Telefono;
            usuario.Direccion = dto.Direccion;
            usuario.Ciudad = dto.Ciudad;
            usuario.Estado = dto.Estado;
            usuario.CodigoPostal = dto.CodigoPostal;
            usuario.UrlFotoPerfil = dto.UrlFotoPerfil;
            usuario.NotificacionesPush = dto.NotificacionesPush;
            usuario.NotificacionesEmail = dto.NotificacionesEmail;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Perfil actualizado exitosamente",
                data = new
                {
                    usuario.NombreCompleto,
                    usuario.Email,
                    usuario.PhoneNumber,
                    usuario.UrlFotoPerfil
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil del usuario");
            return StatusCode(500, new { message = "Error al actualizar el perfil" });
        }
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado
    /// POST /api/me/cambiar-password
    /// </summary>
    [HttpPost("cambiar-password")]
    public async Task<ActionResult> CambiarPassword([FromBody] MeChangePasswordDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _userManager.FindByIdAsync(userId!);
            
            if (usuario == null)
                return NotFound(new { success = false, message = "Usuario no encontrado" });

            var result = await _userManager.ChangePasswordAsync(usuario, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error al cambiar la contraseña",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new
            {
                success = true,
                message = "Contraseña actualizada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña");
            return StatusCode(500, new { success = false, message = "Error al cambiar la contraseña" });
        }
    }

    /// <summary>
    /// Elimina la cuenta del usuario (Soft Delete)
    /// DELETE /api/me
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> EliminarCuenta()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado" });

            // Soft Delete: Cambiar estatus a Cancelado/Eliminado (asumimos 0 o un estatus específico)
            // Verificar EstatusGeneral para "Eliminado" o "Inactivo". Usaremos 0 como inactivo/eliminado por convención.
            usuario.Estatus = 0; 
            usuario.Email = $"deleted_{Guid.NewGuid()}_{usuario.Email}"; // Anonimizar para permitir re-registro si es necesario
            usuario.UserName = usuario.Email;
            usuario.NormalizedEmail = usuario.Email.ToUpper();
            usuario.NormalizedUserName = usuario.UserName.ToUpper();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Cuenta eliminada exitosamente. Esperamos verte pronto."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cuenta");
            return StatusCode(500, new { message = "Error al eliminar la cuenta" });
        }
    }
}
