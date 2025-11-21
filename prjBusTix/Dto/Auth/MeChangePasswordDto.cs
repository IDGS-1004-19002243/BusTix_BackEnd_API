using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Auth;

public class MeChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;
    
    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
