namespace prjBusTix.Dto.Boletos;

public class BoletoCompletoDto
{
    public int BoletoID { get; set; }
    public string CodigoBoleto { get; set; } = string.Empty;
    public string CodigoQR { get; set; } = string.Empty;
    public string? NumeroAsiento { get; set; }
    public string NombrePasajero { get; set; } = string.Empty;
    public decimal PrecioTotal { get; set; }
    public int Estatus { get; set; }
    public string EstatusNombre { get; set; } = string.Empty;
    public string? ParadaAbordaje { get; set; }
    public decimal? ParadaAbordajeLatitud { get; set; }
    public decimal? ParadaAbordajeLongitud { get; set; }
    
    public BoletoDetalleViajeDto DetalleViaje { get; set; } = new();
}
