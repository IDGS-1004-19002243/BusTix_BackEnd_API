using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Auth;

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    
    public string NewPassword { get; set; } = string.Empty;
}