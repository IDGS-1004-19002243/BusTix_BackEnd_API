using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Notificaciones;

/// <summary>
/// DTO para registrar token de dispositivo push
/// </summary>
public class RegistrarDispositivoDto
{
    [Required]
    [MaxLength(500)]
    public string TokenPush { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? TipoDispositivo { get; set; } // "iOS", "Android", "Web"
    
    [MaxLength(50)]
    public string? Plataforma { get; set; } // "FCM", "APNS"
}

