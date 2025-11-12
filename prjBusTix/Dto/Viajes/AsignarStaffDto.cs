using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Viajes;

/// <summary>
/// DTO para asignar staff a un viaje
/// </summary>
public class AsignarStaffDto
{
    [Required(ErrorMessage = "El ID del staff es requerido")]
    public string StaffID { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol en viaje es requerido")]
    [MaxLength(50)]
    public string RolEnViaje { get; set; } = string.Empty; // Supervisor, Validador, Auxiliar

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}

