// filepath: c:\Users\sierr\source\repos\BusTix_BackEnd_API\prjBusTix\Dto\Auth\AdminResendConfirmationDto.cs
using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Auth;

public class AdminResendConfirmationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Nota { get; set; }
}

