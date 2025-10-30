using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Auth;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}