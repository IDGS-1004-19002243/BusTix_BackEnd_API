using prjBusTix.Model;

namespace prjBusTix.Services;

/// <summary>
/// Interfaz para el servicio de notificaciones
/// </summary>
public interface INotificacionService
{
    /// <summary>
    /// Envía una notificación a un usuario específico
    /// </summary>
    Task<bool> EnviarNotificacionAsync(Notificacion notificacion);
    
    /// <summary>
    /// Envía notificación push a través de Firebase/APNS
    /// </summary>
    Task<bool> EnviarPushAsync(string usuarioId, string titulo, string mensaje, Dictionary<string, string>? data = null);
    
    /// <summary>
    /// Envía notificación por email
    /// </summary>
    Task<bool> EnviarEmailAsync(string email, string titulo, string mensaje);
    
    /// <summary>
    /// Envía notificación en tiempo real vía SignalR
    /// </summary>
    Task<bool> EnviarNotificacionTiempoRealAsync(string usuarioId, object notificacion);
    
    /// <summary>
    /// Crea notificación de confirmación de compra
    /// </summary>
    Task EnviarConfirmacionCompraAsync(int boletoId);
    
    /// <summary>
    /// Crea recordatorio automático de viaje
    /// </summary>
    Task EnviarRecordatorioViajeAsync(int viajeId, int horasAntes);
    
    /// <summary>
    /// Obtiene tokens de dispositivos activos del usuario
    /// </summary>
    Task<List<string>> ObtenerTokensDispositivosAsync(string usuarioId);
}

