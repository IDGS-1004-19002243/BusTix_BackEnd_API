using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Auth;

public class UpdateProfileDto
{
    [Required]
    [StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Phone]
    public string? Telefono { get; set; }

    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public string? CodigoPostal { get; set; }
    
    public string? UrlFotoPerfil { get; set; }
    
    public bool NotificacionesPush { get; set; }
    public bool NotificacionesEmail { get; set; }
}
