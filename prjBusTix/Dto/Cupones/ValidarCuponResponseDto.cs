namespace prjBusTix.Dto.Cupones;

public class ValidarCuponResponseDto
{
    public bool Valido { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CuponID { get; set; }
    public string? Codigo { get; set; }
    public string? TipoDescuento { get; set; }
    public decimal? ValorDescuento { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public int? UsosMaximos { get; set; }
    public int? UsosRealizados { get; set; }
}
