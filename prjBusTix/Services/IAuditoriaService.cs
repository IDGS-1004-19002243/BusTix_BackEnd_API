using prjBusTix.Model;

namespace prjBusTix.Services;

/// <summary>
/// Interfaz para el servicio de auditoría
/// </summary>
public interface IAuditoriaService
{
    /// <summary>
    /// Registra un cambio en la auditoría
    /// </summary>
    Task RegistrarCambioAsync(
        string tabla, 
        string registroId, 
        string accion, 
        string? datosPrevios = null, 
        string? datosPosteriores = null);
    
    /// <summary>
    /// Obtiene el historial de auditoría
    /// </summary>
    Task<List<AuditoriaCambio>> ObtenerHistorialAsync(
        string? tabla = null,
        string? usuarioId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        int pagina = 1,
        int tamanoPagina = 50);
    
    /// <summary>
    /// Obtiene auditoría de un registro específico
    /// </summary>
    Task<List<AuditoriaCambio>> ObtenerHistorialRegistroAsync(string tabla, string registroId);
}

