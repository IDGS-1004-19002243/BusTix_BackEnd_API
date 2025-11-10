using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Incidencias;

/// <summary>
/// DTO para crear una nueva incidencia (usado por Staff desde móvil)
/// </summary>
public class CrearIncidenciaDto
{
    /// <summary>
    /// ID del tipo de incidencia (obligatorio)
    /// </summary>
    [Required(ErrorMessage = "El tipo de incidencia es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del tipo de incidencia debe ser mayor a 0")]
    public int TipoIncidenciaID { get; set; }
    
    /// <summary>
    /// ID del viaje asociado (opcional si no está en un viaje específico)
    /// </summary>
    public int? ViajeID { get; set; }
    
    /// <summary>
    /// ID de la unidad asociada (opcional)
    /// </summary>
    public int? UnidadID { get; set; }
    
    /// <summary>
    /// Título corto de la incidencia
    /// </summary>
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(256, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 256 caracteres")]
    public string Titulo { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción detallada de la incidencia
    /// </summary>
    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 4000 caracteres")]
    public string Descripcion { get; set; } = string.Empty;
    
    /// <summary>
    /// Prioridad de la incidencia: Baja, Media, Alta, Crítica
    /// </summary>
    [Required(ErrorMessage = "La prioridad es obligatoria")]
    [RegularExpression("^(Baja|Media|Alta|Crítica)$", ErrorMessage = "La prioridad debe ser: Baja, Media, Alta o Crítica")]
    public string Prioridad { get; set; } = "Media";
    
    /// <summary>
    /// URL de evidencia fotográfica (opcional) - para adjuntar fotos
    /// </summary>
    [Url(ErrorMessage = "Debe ser una URL válida")]
    [MaxLength(500, ErrorMessage = "La URL no puede exceder 500 caracteres")]
    public string? UrlEvidencia { get; set; }
}
