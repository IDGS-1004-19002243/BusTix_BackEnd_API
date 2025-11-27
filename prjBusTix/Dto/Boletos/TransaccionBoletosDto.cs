namespace prjBusTix.Dto.Boletos;

public class TransaccionBoletosDto
{
    public int PagoID { get; set; }
    public string CodigoPago { get; set; } = string.Empty;
    public string? TransaccionID { get; set; } // ID del Gateway
    public DateTime FechaPago { get; set; }
    public decimal MontoTotal { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    
    public List<BoletoCompletoDto> Boletos { get; set; } = new();
}
