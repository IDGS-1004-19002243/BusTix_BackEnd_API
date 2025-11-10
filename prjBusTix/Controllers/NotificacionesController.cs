using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Notificaciones;
using prjBusTix.Model;
using prjBusTix.Services;
using System.Security.Claims;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly INotificacionService _notificacionService;
    private readonly UserManager<ClApplicationUser> _userManager;
    private readonly ILogger<NotificacionesController> _logger;
    
    public NotificacionesController(
        AppDbContext context,
        INotificacionService notificacionService,
        UserManager<ClApplicationUser> userManager,
        ILogger<NotificacionesController> logger)
    {
        _context = context;
        _notificacionService = notificacionService;
        _userManager = userManager;
        _logger = logger;
    }
    
    /// <summary>
    /// Envía notificación broadcast (masiva)
    /// POST /api/notificaciones/broadcast
    /// </summary>
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> EnviarBroadcast([FromBody] NotificacionBroadcastDto dto)
    {
        try
        {
            List<string> destinatarios = new List<string>();
            
            // Caso 1: Notificación a pasajeros de un viaje específico
            if (dto.ViajeID.HasValue)
            {
                var viaje = await _context.Viajes
                    .Include(v => v.PlantillaRuta)
                    .FirstOrDefaultAsync(v => v.ViajeID == dto.ViajeID.Value);
                
                if (viaje == null)
                    return NotFound(new { message = "Viaje no encontrado" });
                
                // Obtener IDs de clientes con boletos pagados en ese viaje
                var clientesIds = await (
                    from b in _context.Boletos
                    where b.ViajeID == dto.ViajeID.Value && b.Estatus == 10
                    select b.ClienteID
                ).Distinct().ToListAsync();
                
                destinatarios.AddRange(clientesIds);
                
                _logger.LogInformation(
                    "Broadcast a {Count} pasajeros del viaje {CodigoViaje}",
                    destinatarios.Count, viaje.CodigoViaje);
            }
            // Caso 2: Notificación por roles
            else if (dto.Roles != null && dto.Roles.Any())
            {
                foreach (var rol in dto.Roles)
                {
                    var usuariosEnRol = await _userManager.GetUsersInRoleAsync(rol);
                    destinatarios.AddRange(usuariosEnRol.Select(u => u.Id));
                }
                destinatarios = destinatarios.Distinct().ToList();
                
                var rolesString = string.Join(", ", dto.Roles);
                _logger.LogInformation(
                    "Broadcast a {Count} usuarios con roles: {Roles}",
                    destinatarios.Count, rolesString);
            }
            // Caso 3: IDs específicos
            else if (dto.UsuariosIDs != null && dto.UsuariosIDs.Any())
            {
                destinatarios = dto.UsuariosIDs;
                
                _logger.LogInformation(
                    "Broadcast a {Count} usuarios específicos",
                    destinatarios.Count);
            }
            else
            {
                return BadRequest(new { message = "Debe especificar ViajeID, Roles o UsuariosIDs" });
            }
            
            if (!destinatarios.Any())
            {
                return BadRequest(new { message = "No se encontraron destinatarios" });
            }
            
            // Crear notificaciones para cada destinatario
            var notificaciones = new List<Notificacion>();
            
            foreach (var usuarioId in destinatarios)
            {
                var notificacion = new Notificacion
                {
                    UsuarioID = usuarioId,
                    ViajeID = dto.ViajeID,
                    Titulo = dto.Titulo,
                    Mensaje = dto.Mensaje,
                    TipoNotificacion = dto.TipoNotificacion,
                    EnviarPush = dto.EnviarPush,
                    EnviarEmail = dto.EnviarEmail,
                    FechaCreacion = DateTime.Now
                };
                
                notificaciones.Add(notificacion);
            }
            
            _context.Notificaciones.AddRange(notificaciones);
            await _context.SaveChangesAsync();
            
            // Enviar notificaciones en segundo plano
            int exitosas = 0;
            foreach (var notif in notificaciones)
            {
                var resultado = await _notificacionService.EnviarNotificacionAsync(notif);
                if (resultado) exitosas++;
            }
            
            return Ok(new
            {
                message = "Notificaciones enviadas",
                totalDestinatarios = destinatarios.Count,
                notificacionesCreadas = notificaciones.Count,
                notificacionesEnviadas = exitosas
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar broadcast");
            return StatusCode(500, new { message = "Error al enviar notificaciones masivas" });
        }
    }
    
    /// <summary>
    /// Crea una notificación individual
    /// POST /api/notificaciones
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<NotificacionResponseDto>> CrearNotificacion([FromBody] CrearNotificacionDto dto)
    {
        try
        {
            var notificacion = new Notificacion
            {
                UsuarioID = dto.UsuarioID,
                ViajeID = dto.ViajeID,
                BoletoID = dto.BoletoID,
                Titulo = dto.Titulo,
                Mensaje = dto.Mensaje,
                TipoNotificacion = dto.TipoNotificacion,
                EnviarPush = dto.EnviarPush,
                EnviarEmail = dto.EnviarEmail,
                FechaCreacion = DateTime.Now
            };
            
            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
            
            // Enviar la notificación
            await _notificacionService.EnviarNotificacionAsync(notificacion);
            
            return CreatedAtAction(
                nameof(ObtenerNotificacion),
                new { id = notificacion.NotificacionID },
                MapToDto(notificacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear notificación");
            return StatusCode(500, new { message = "Error al crear la notificación" });
        }
    }
    
    /// <summary>
    /// Obtiene el historial de notificaciones del usuario autenticado
    /// GET /api/notificaciones/me
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<NotificacionResponseDto>>> MisNotificaciones(
        [FromQuery] bool? soloNoLeidas = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var query = _context.Notificaciones
                .Include(n => n.Viaje)
                .Include(n => n.Boleto)
                .Where(n => n.UsuarioID == userId);
            
            if (soloNoLeidas == true)
            {
                query = query.Where(n => !n.FueLeida);
            }
            
            var total = await query.CountAsync();
            
            var notificaciones = await query
                .OrderByDescending(n => n.FechaCreacion)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();
            
            var response = notificaciones.Select(MapToDto).ToList();
            
            Response.Headers["X-Total-Count"] = total.ToString();
            Response.Headers["X-Total-Pages"] = ((int)Math.Ceiling(total / (double)porPagina)).ToString();
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones del usuario");
            return StatusCode(500, new { message = "Error al obtener notificaciones" });
        }
    }
    
    /// <summary>
    /// Obtiene una notificación específica
    /// GET /api/notificaciones/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificacionResponseDto>> ObtenerNotificacion(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            
            var notificacion = await _context.Notificaciones
                .Include(n => n.Viaje)
                .Include(n => n.Boleto)
                .FirstOrDefaultAsync(n => n.NotificacionID == id);
            
            if (notificacion == null)
                return NotFound(new { message = "Notificación no encontrada" });
            
            // Verificar que pertenece al usuario o es admin
            if (!isAdmin && notificacion.UsuarioID != userId)
                return Forbid();
            
            return Ok(MapToDto(notificacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificación {NotificacionId}", id);
            return StatusCode(500, new { message = "Error al obtener la notificación" });
        }
    }
    
    /// <summary>
    /// Marca una notificación como leída
    /// PUT /api/notificaciones/{id}/leer
    /// </summary>
    [HttpPut("{id}/leer")]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.NotificacionID == id && n.UsuarioID == userId);
            
            if (notificacion == null)
                return NotFound(new { 
                    success = false,
                    message = "Notificación no encontrada" 
                });
            
            if (notificacion.FueLeida)
            {
                return Ok(new { 
                    success = true,
                    message = "La notificación ya estaba marcada como leída",
                    fechaLectura = notificacion.FechaLectura
                });
            }
            
            notificacion.FueLeida = true;
            notificacion.FechaLectura = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Notificación {NotificacionId} marcada como leída por usuario {UserId}",
                id, userId);
            
            return Ok(new { 
                success = true,
                message = "Notificación marcada como leída",
                fechaLectura = notificacion.FechaLectura
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación {NotificacionId} como leída", id);
            return StatusCode(500, new { 
                success = false,
                message = "Error al actualizar la notificación" 
            });
        }
    }
    
    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas
    /// PUT /api/notificaciones/marcar-todas-leidas
    /// </summary>
    [HttpPut("marcar-todas-leidas")]
    public async Task<IActionResult> MarcarTodasComoLeidas()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioID == userId && !n.FueLeida)
                .ToListAsync();
            
            if (!notificacionesNoLeidas.Any())
            {
                return Ok(new { 
                    success = true,
                    message = "No hay notificaciones sin leer",
                    actualizadas = 0
                });
            }
            
            foreach (var notif in notificacionesNoLeidas)
            {
                notif.FueLeida = true;
                notif.FechaLectura = DateTime.Now;
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Usuario {UserId} marcó {Count} notificaciones como leídas",
                userId, notificacionesNoLeidas.Count);
            
            return Ok(new { 
                success = true,
                message = $"{notificacionesNoLeidas.Count} notificaciones marcadas como leídas",
                actualizadas = notificacionesNoLeidas.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar todas las notificaciones como leídas");
            return StatusCode(500, new { 
                success = false,
                message = "Error al actualizar las notificaciones" 
            });
        }
    }
    
    /// <summary>
    /// Obtiene el contador de notificaciones no leídas
    /// GET /api/notificaciones/no-leidas/count
    /// </summary>
    [HttpGet("no-leidas/count")]
    public async Task<ActionResult<int>> GetCountNoLeidas()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var count = await _context.Notificaciones
                .CountAsync(n => n.UsuarioID == userId && !n.FueLeida);
            
            return Ok(new { 
                success = true,
                count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar notificaciones no leídas");
            return StatusCode(500, new { 
                success = false,
                message = "Error al obtener el contador" 
            });
        }
    }
    
    /// <summary>
    /// Elimina una notificación
    /// DELETE /api/notificaciones/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarNotificacion(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.NotificacionID == id && n.UsuarioID == userId);
            
            if (notificacion == null)
                return NotFound(new { 
                    success = false,
                    message = "Notificación no encontrada" 
                });
            
            _context.Notificaciones.Remove(notificacion);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Notificación {NotificacionId} eliminada por usuario {UserId}",
                id, userId);
            
            return Ok(new { 
                success = true,
                message = "Notificación eliminada" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar notificación {NotificacionId}", id);
            return StatusCode(500, new { 
                success = false,
                message = "Error al eliminar la notificación" 
            });
        }
    }
    
    // Método auxiliar para mapear a DTO
    private static NotificacionResponseDto MapToDto(Notificacion notificacion)
    {
        return new NotificacionResponseDto
        {
            NotificacionID = notificacion.NotificacionID,
            Titulo = notificacion.Titulo,
            Mensaje = notificacion.Mensaje,
            TipoNotificacion = notificacion.TipoNotificacion ?? "General",
            FechaCreacion = notificacion.FechaCreacion,
            FueLeida = notificacion.FueLeida,
            FechaLectura = notificacion.FechaLectura,
            FueEnviada = notificacion.FueEnviada,
            FechaEnvio = notificacion.FechaEnvio,
            ViajeID = notificacion.ViajeID,
            BoletoID = notificacion.BoletoID
        };
    }
}


