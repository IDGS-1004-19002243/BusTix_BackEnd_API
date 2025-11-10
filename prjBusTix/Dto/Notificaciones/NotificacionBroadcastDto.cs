using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Notificaciones;

/// <summary>
/// DTO para enviar notificación broadcast (masiva)
/// </summary>
public class NotificacionBroadcastDto
{
    [Required]
    [MaxLength(256)]
    public string Titulo { get; set; } = string.Empty;
    
    [Required]
    public string Mensaje { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? TipoNotificacion { get; set; } = "Broadcast";
    
    /// <summary>
    /// Si se especifica, envía solo a pasajeros de este viaje
    /// </summary>
    public int? ViajeID { get; set; }
    
    /// <summary>
    /// Filtrar por roles: "Admin", "Manager", "Operator", "User"
    /// </summary>
    public List<string>? Roles { get; set; }
    
    /// <summary>
    /// IDs específicos de usuarios (opcional)
    /// </summary>
    public List<string>? UsuariosIDs { get; set; }
    
    public bool EnviarPush { get; set; } = true;
    public bool EnviarEmail { get; set; } = false;
}

