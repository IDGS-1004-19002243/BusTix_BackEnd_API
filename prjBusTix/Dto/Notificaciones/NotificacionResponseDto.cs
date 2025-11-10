namespace prjBusTix.Dto.Notificaciones;

/// <summary>
/// DTO de respuesta con información de notificación
/// </summary>
public class NotificacionResponseDto
{
    public int NotificacionID { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? TipoNotificacion { get; set; }
    
    public int? ViajeID { get; set; }
    public string? CodigoViaje { get; set; }
    
    public int? BoletoID { get; set; }
    public string? CodigoBoleto { get; set; }
    
    public bool FueEnviada { get; set; }
    public DateTime? FechaEnvio { get; set; }
    
    public bool FueLeida { get; set; }
    public DateTime? FechaLectura { get; set; }
    
    public DateTime FechaCreacion { get; set; }
}

