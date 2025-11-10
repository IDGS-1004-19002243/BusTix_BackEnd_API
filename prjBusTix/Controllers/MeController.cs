using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Boletos;
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

    public MeController(AppDbContext context, ILogger<MeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el historial de boletos del usuario autenticado
    /// GET /api/me/boletos
    /// </summary>
    [HttpGet("boletos")]
    public async Task<ActionResult<IEnumerable<BoletoResponseDto>>> GetMisBoletos(
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
                .Include(b => b.ParadaAbordaje)
                .Include(b => b.EstatusNavigation)
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

            var response = boletos.Select(b => new BoletoResponseDto
            {
                BoletoID = b.BoletoID,
                CodigoBoleto = b.CodigoBoleto,
                CodigoQR = b.CodigoQR,
                ViajeID = b.ViajeID,
                CodigoViaje = b.Viaje.CodigoViaje,
                CiudadOrigen = b.Viaje.PlantillaRuta.CiudadOrigen,
                CiudadDestino = b.Viaje.PlantillaRuta.CiudadDestino,
                FechaSalida = b.Viaje.FechaSalida,
                NumeroAsiento = b.NumeroAsiento,
                NombrePasajero = b.NombrePasajero,
                EmailPasajero = b.EmailPasajero,
                TelefonoPasajero = b.TelefonoPasajero,
                PrecioBase = b.PrecioBase,
                Descuento = b.Descuento,
                CargoServicio = b.CargoServicio,
                IVA = b.IVA,
                PrecioTotal = b.PrecioTotal,
                Estatus = b.Estatus,
                EstatusNombre = b.EstatusNavigation.Nombre,
                FechaCompra = b.FechaCompra,
                FechaValidacion = b.FechaValidacion,
                ParadaAbordaje = b.ParadaAbordaje?.NombreParada,
                HoraEstimadaAbordaje = b.ParadaAbordaje?.HoraEstimadaLlegada
            }).ToList();

            return Ok(new
            {
                success = true,
                data = response,
                total = response.Count
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
                    estatus = usuario.EstatusNavigation.Nombre,
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
}

