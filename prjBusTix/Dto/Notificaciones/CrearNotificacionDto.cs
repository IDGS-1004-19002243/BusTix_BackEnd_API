using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Notificaciones;

/// <summary>
/// DTO para crear una notificación individual
/// </summary>
public class CrearNotificacionDto
{
    [Required]
    public string UsuarioID { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(256)]
    public string Titulo { get; set; } = string.Empty;
    
    [Required]
    public string Mensaje { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? TipoNotificacion { get; set; }
    
    public int? ViajeID { get; set; }
    public int? BoletoID { get; set; }
    
    public bool EnviarPush { get; set; } = true;
    public bool EnviarEmail { get; set; } = false;
}

