using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Hubs;
using prjBusTix.Model;
using System.Text.Json;

namespace prjBusTix.Services;

/// <summary>
/// Servicio para gestión de notificaciones
/// Integra con SignalR (tiempo real), Email y opcionalmente Firebase
/// </summary>
public class NotificacionService : INotificacionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificacionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificacionesHub> _hubContext;
    
    public NotificacionService(
        AppDbContext context,
        ILogger<NotificacionService> logger,
        IConfiguration configuration,
        IHubContext<NotificacionesHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;
    }
    
    /// <summary>
    /// Envía una notificación completa (SignalR + push + email según configuración)
    /// </summary>
    public async Task<bool> EnviarNotificacionAsync(Notificacion notificacion)
    {
        try
        {
            bool exitoTiempoReal = false;
            bool exitoPush = true;
            bool exitoEmail = true;
            
            // 1. ENVIAR VÍA SIGNALR (Tiempo Real) - SIEMPRE
            exitoTiempoReal = await EnviarNotificacionTiempoRealAsync(
                notificacion.UsuarioID,
                new
                {
                    notificacionId = notificacion.NotificacionID,
                    titulo = notificacion.Titulo,
                    mensaje = notificacion.Mensaje,
                    tipo = notificacion.TipoNotificacion ?? "general",
                    fechaCreacion = notificacion.FechaCreacion,
                    viajeId = notificacion.ViajeID,
                    boletoId = notificacion.BoletoID
                });
            
            // 2. Enviar push si está habilitado
            if (notificacion.EnviarPush)
            {
                // Verificar preferencias del usuario
                var usuario = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == notificacion.UsuarioID);
                
                if (usuario?.NotificacionesPush == true)
                {
                    var data = new Dictionary<string, string>
                    {
                        { "notificacionId", notificacion.NotificacionID.ToString() },
                        { "tipo", notificacion.TipoNotificacion ?? "general" }
                    };
                    
                    if (notificacion.ViajeID.HasValue)
                        data["viajeId"] = notificacion.ViajeID.Value.ToString();
                    
                    if (notificacion.BoletoID.HasValue)
                        data["boletoId"] = notificacion.BoletoID.Value.ToString();
                    
                    exitoPush = await EnviarPushAsync(
                        notificacion.UsuarioID,
                        notificacion.Titulo,
                        notificacion.Mensaje,
                        data
                    );
                }
            }
            
            // 3. Enviar email si está habilitado
            if (notificacion.EnviarEmail)
            {
                var usuario = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == notificacion.UsuarioID);
                
                if (usuario?.NotificacionesEmail == true && !string.IsNullOrEmpty(usuario.Email))
                {
                    exitoEmail = await EnviarEmailAsync(
                        usuario.Email,
                        notificacion.Titulo,
                        notificacion.Mensaje
                    );
                }
            }
            
            // Actualizar estado de notificación
            notificacion.FueEnviada = exitoTiempoReal || exitoPush || exitoEmail;
            notificacion.FechaEnvio = DateTime.Now;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Notificación {NotificacionId} enviada. TiempoReal: {TR}, Push: {Push}, Email: {Email}",
                notificacion.NotificacionID, exitoTiempoReal, exitoPush, exitoEmail);
            
            return exitoTiempoReal || exitoPush || exitoEmail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación {NotificacionID}", notificacion.NotificacionID);
            return false;
        }
    }
    
    /// <summary>
    /// Envía notificación en tiempo real vía SignalR
    /// </summary>
    public async Task<bool> EnviarNotificacionTiempoRealAsync(string usuarioId, object notificacion)
    {
        try
        {
            // Verificar si el usuario está conectado
            if (!NotificacionesHub.IsUserConnected(usuarioId))
            {
                _logger.LogInformation(
                    "Usuario {UserId} no está conectado a SignalR, notificación guardada en BD",
                    usuarioId);
                return false;
            }
            
            // Enviar a todos los dispositivos/conexiones del usuario
            await _hubContext.Clients
                .Group($"user-{usuarioId}")
                .SendAsync("RecibirNotificacion", notificacion);
            
            _logger.LogInformation(
                "Notificación enviada en tiempo real a usuario {UserId}",
                usuarioId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación en tiempo real a usuario {UserId}", usuarioId);
            return false;
        }
    }
    
    /// <summary>
    /// Envía push notification (actualmente solo logging, Firebase desactivado por costos)
    /// </summary>
    public async Task<bool> EnviarPushAsync(
        string usuarioId,
        string titulo,
        string mensaje,
        Dictionary<string, string>? data = null)
    {
        try
        {
            // Obtener tokens de dispositivos del usuario
            var tokens = await ObtenerTokensDispositivosAsync(usuarioId);
            
            if (!tokens.Any())
            {
                _logger.LogWarning("Usuario {UsuarioId} no tiene dispositivos registrados", usuarioId);
                return false;
            }
            
            // Por ahora solo registramos en logs (Firebase desactivado)
            _logger.LogInformation(
                "PUSH NOTIFICATION - Usuario: {UsuarioId}, Dispositivos: {Count}, Título: {Titulo}, Mensaje: {Mensaje}",
                usuarioId, tokens.Count, titulo, mensaje);
            
            if (data != null && data.Any())
            {
                _logger.LogInformation("PUSH DATA: {Data}", string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}")));
            }
            
            // TODO: Activar cuando se configure Firebase
            /*
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensaje
                },
                Data = data
            };
            
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            return response.SuccessCount > 0;
            */
            
            // Simulación exitosa
            await Task.Delay(50);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar push notification a usuario {UsuarioId}", usuarioId);
            return false;
        }
    }
    
    /// <summary>
    /// Envía notificación por email usando MailKit
    /// </summary>
    public async Task<bool> EnviarEmailAsync(string email, string titulo, string mensaje)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:Server"];
            var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            var smtpUser = _configuration["EmailSettings:SenderEmail"];
            var smtpPass = _configuration["EmailSettings:Password"];
            
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser))
            {
                _logger.LogWarning("Configuración de email no encontrada, omitiendo envío");
                return false;
            }
            
            _logger.LogInformation("Enviando email a {Email}: {Titulo}", email, titulo);
            
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress("BusTix Notificaciones", smtpUser));
            message.To.Add(new MimeKit.MailboxAddress("", email));
            message.Subject = titulo;
            
            var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #007bff; padding: 20px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>🚌 BusTix</h1>
                </div>
                <div style='padding: 30px; background-color: #f8f9fa;'>
                    <h2 style='color: #333;'>{titulo}</h2>
                    <p style='color: #666; font-size: 16px; line-height: 1.6;'>{mensaje}</p>
                </div>
                <div style='background-color: #343a40; padding: 15px; text-align: center;'>
                    <p style='color: #adb5bd; font-size: 12px; margin: 0;'>
                        Este es un correo automático, por favor no responder.
                    </p>
                </div>
            </div>";
            
            message.Body = new MimeKit.TextPart("html") { Text = htmlBody };
            
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Email enviado exitosamente a {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {Email}", email);
            return false;
        }
    }
    
    /// <summary>
    /// Crea y envía notificación automática de confirmación de compra
    /// </summary>
    public async Task EnviarConfirmacionCompraAsync(int boletoId)
    {
        try
        {
            var boleto = await _context.Boletos
                .Include(b => b.Cliente)
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .FirstOrDefaultAsync(b => b.BoletoID == boletoId);
            
            if (boleto == null)
            {
                _logger.LogWarning("Boleto {BoletoId} no encontrado para notificación", boletoId);
                return;
            }
            
            var notificacion = new Notificacion
            {
                UsuarioID = boleto.ClienteID,
                ViajeID = boleto.ViajeID,
                BoletoID = boleto.BoletoID,
                Titulo = "¡Compra Confirmada! 🎫",
                Mensaje = $"Tu boleto para el viaje de {boleto.Viaje.PlantillaRuta.CiudadOrigen} " +
                         $"a {boleto.Viaje.PlantillaRuta.CiudadDestino} " +
                         $"el {boleto.Viaje.FechaSalida:dd/MM/yyyy HH:mm} ha sido confirmado. " +
                         $"Código: {boleto.CodigoBoleto}",
                TipoNotificacion = "ConfirmacionCompra",
                EnviarPush = true,
                EnviarEmail = true,
                FechaCreacion = DateTime.Now
            };
            
            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
            
            await EnviarNotificacionAsync(notificacion);
            
            _logger.LogInformation(
                "Notificación de confirmación creada para boleto {BoletoId}",
                boletoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar confirmación de compra para boleto {BoletoId}", boletoId);
        }
    }
    
    /// <summary>
    /// Crea y envía recordatorios automáticos de viaje
    /// </summary>
    public async Task EnviarRecordatorioViajeAsync(int viajeId, int horasAntes)
    {
        try
        {
            var viaje = await _context.Viajes
                .Include(v => v.PlantillaRuta)
                .FirstOrDefaultAsync(v => v.ViajeID == viajeId);
            
            if (viaje == null)
            {
                _logger.LogWarning("Viaje {ViajeId} no encontrado", viajeId);
                return;
            }
            
            // Obtener todos los pasajeros del viaje
            var boletos = await _context.Boletos
                .Where(b => b.ViajeID == viajeId && b.Estatus == 10) // Solo pagados
                .ToListAsync();
            
            var notificaciones = new List<Notificacion>();
            
            foreach (var boleto in boletos)
            {
                var notificacion = new Notificacion
                {
                    UsuarioID = boleto.ClienteID,
                    ViajeID = viaje.ViajeID,
                    BoletoID = boleto.BoletoID,
                    Titulo = $"Recordatorio: Tu viaje es en {horasAntes} horas ⏰",
                    Mensaje = $"Tu viaje de {viaje.PlantillaRuta.CiudadOrigen} " +
                             $"a {viaje.PlantillaRuta.CiudadDestino} " +
                             $"sale el {viaje.FechaSalida:dd/MM/yyyy} a las {viaje.FechaSalida:HH:mm}. " +
                             $"¡No olvides tu boleto! Código: {boleto.CodigoBoleto}",
                    TipoNotificacion = "Recordatorio",
                    EnviarPush = true,
                    EnviarEmail = false,
                    FechaCreacion = DateTime.Now
                };
                
                notificaciones.Add(notificacion);
            }
            
            _context.Notificaciones.AddRange(notificaciones);
            await _context.SaveChangesAsync();
            
            // Enviar en segundo plano
            foreach (var notif in notificaciones)
            {
                _ = Task.Run(() => EnviarNotificacionAsync(notif));
            }
            
            _logger.LogInformation(
                "Enviados {Count} recordatorios para viaje {ViajeId}",
                notificaciones.Count, viajeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar recordatorios de viaje {ViajeId}", viajeId);
        }
    }
    
    /// <summary>
    /// Obtiene los tokens de dispositivos activos del usuario
    /// </summary>
    public async Task<List<string>> ObtenerTokensDispositivosAsync(string usuarioId)
    {
        try
        {
            return await _context.DispositivosUsuario
                .Where(d => d.UsuarioID == usuarioId && d.EsActivo)
                .Select(d => d.TokenPush)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tokens de usuario {UsuarioId}", usuarioId);
            return new List<string>();
        }
    }
}

