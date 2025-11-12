namespace prjBusTix.Dto.Cupones;

public class CuponResponseDto
{
    public int CuponID { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string TipoDescuento { get; set; } = string.Empty;
    public decimal ValorDescuento { get; set; }
    public int? UsosMaximos { get; set; }
    public int UsosRealizados { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool EsActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool EstaVigente { get; set; }
}
