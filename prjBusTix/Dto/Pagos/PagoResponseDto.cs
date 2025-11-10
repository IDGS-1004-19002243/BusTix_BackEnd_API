namespace prjBusTix.Dto.Pagos;

/// <summary>
/// DTO con información del pago realizado
/// </summary>
public class PagoResponseDto
{
    public int PagoID { get; set; }
    public string CodigoPago { get; set; } = string.Empty;
    public string? TransaccionID { get; set; }
    
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string? Proveedor { get; set; }
    
    public int Estatus { get; set; }
    public string EstatusNombre { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
    
    public List<int> BoletosIDs { get; set; } = new List<int>();
}

