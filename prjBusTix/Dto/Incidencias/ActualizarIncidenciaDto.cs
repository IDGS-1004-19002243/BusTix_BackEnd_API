using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Incidencias;

/// <summary>
/// DTO para actualizar el estado de una incidencia (usado por Administradores)
/// </summary>
public class ActualizarIncidenciaDto
{
    /// <summary>
    /// ID del estatus a actualizar
    /// Los estatus típicos son:
    /// 1 = Abierta/Nueva
    /// 2 = En Proceso/En Atención
    /// 3 = Resuelta
    /// 4 = Cerrada
    /// 5 = Cancelada
    /// </summary>
    [Required(ErrorMessage = "El estatus es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El estatus debe ser mayor a 0")]
    public int Estatus { get; set; }
    
    /// <summary>
    /// Prioridad actualizada: Baja, Media, Alta, Crítica
    /// </summary>
    [RegularExpression("^(Baja|Media|Alta|Crítica)$", ErrorMessage = "La prioridad debe ser: Baja, Media, Alta o Crítica")]
    [MaxLength(50, ErrorMessage = "La prioridad no puede exceder 50 caracteres")]
    public string? Prioridad { get; set; }
    
    /// <summary>
    /// ID del usuario al que se asigna la incidencia (staff o técnico responsable)
    /// </summary>
    [MaxLength(450, ErrorMessage = "El ID del usuario no puede exceder 450 caracteres")]
    public string? AsignadoA { get; set; }
    
    /// <summary>
    /// Notas adicionales sobre la actualización o resolución
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Las notas no pueden exceder 2000 caracteres")]
    public string? Notas { get; set; }
}

