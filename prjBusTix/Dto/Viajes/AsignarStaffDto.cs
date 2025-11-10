using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Viajes;

public class AsignarStaffDto
{
    [Required]
    [MaxLength(450)]
    public string StaffID { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string RolEnViaje { get; set; } = string.Empty; // "Chofer", "Auxiliar", "Validador", "Supervisor"
    
    [MaxLength(500)]
    public string? Observaciones { get; set; }
}

public class StaffViajeResponseDto
{
    public int AsignacionID { get; set; }
    public int ViajeID { get; set; }
    public string StaffID { get; set; } = string.Empty;
    public string StaffNombre { get; set; } = string.Empty;
    public string? StaffEmail { get; set; }
    public string? StaffTelefono { get; set; }
    public string RolEnViaje { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
    public string? Observaciones { get; set; }
}

