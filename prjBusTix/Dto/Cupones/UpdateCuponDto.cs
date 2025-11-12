namespace prjBusTix.Dto.Cupones;

public class UpdateCuponDto
{
    public string? Descripcion { get; set; }
    public string? TipoDescuento { get; set; } // Porcentaje, MontoFijo
    public decimal? ValorDescuento { get; set; }
    public int? UsosMaximos { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool? EsActivo { get; set; }
}
