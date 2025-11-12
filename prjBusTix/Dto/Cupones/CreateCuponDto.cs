using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Cupones;

public class CreateCuponDto
{
    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Descripcion { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TipoDescuento { get; set; } = "Porcentaje"; // Porcentaje, MontoFijo
    
    [Range(0.01, 999999)]
    public decimal ValorDescuento { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Usos máximos debe ser >= 1")]
    public int? UsosMaximos { get; set; }
    
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    
    public bool EsActivo { get; set; } = true;
}
