using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Roles;

public class CreateRoleDto
{
    [Required]
    [MaxLength(256)]
    public string? RoleName { get; set; }
}

