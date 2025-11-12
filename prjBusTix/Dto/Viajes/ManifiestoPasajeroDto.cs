namespace prjBusTix.Dto.Viajes;

public class ManifiestoPasajeroDto
{
    public int ManifiestoID { get; set; }
    public int BoletoID { get; set; }
    public string CodigoBoleto { get; set; } = string.Empty;
    public string CodigoQR { get; set; } = string.Empty;
    public string NombrePasajero { get; set; } = string.Empty;
    public string? EmailPasajero { get; set; }
    public string? TelefonoPasajero { get; set; }
    public string? NumeroAsiento { get; set; }
    public int EstatusAbordaje { get; set; }
    public string EstatusAbordajeNombre { get; set; } = string.Empty;
    public DateTime? FechaAbordaje { get; set; }
    public bool FueValidado { get; set; }
    public DateTime? FechaValidacion { get; set; }
    public string? ValidadoPor { get; set; }
    public DateTime? FechaCheckIn { get; set; }
    public int? EstatusCheckIn { get; set; }
    public decimal? CheckInLat { get; set; }
    public decimal? CheckInLong { get; set; }
    public string? ObservacionesCheckIn { get; set; }
    public int? ParadaAbordajeID { get; set; }
    public string? ParadaAbordajeNombre { get; set; }
    public DateTime? HoraEstimadaAbordaje { get; set; }
}

