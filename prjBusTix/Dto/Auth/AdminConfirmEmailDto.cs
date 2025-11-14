// filepath: c:\Users\sierr\source\repos\BusTix_BackEnd_API\prjBusTix\Dto\Auth\AdminConfirmEmailDto.cs
using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Auth;

public class AdminConfirmEmailDto
{
    // Puedes usar Email o UserId según lo que prefieras. Aquí usamos Email por simplicidad.
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Opcional: motivo o nota para auditoría
    public string? Nota { get; set; }
}

