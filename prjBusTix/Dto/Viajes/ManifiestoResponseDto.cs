namespace prjBusTix.Dto.Viajes;

/// <summary>
/// DTO para el manifiesto de pasajeros de un viaje
/// </summary>
public class ManifiestoResponseDto
{
    public int ViajeID { get; set; }
    public string CodigoViaje { get; set; } = string.Empty;
    public DateTime FechaSalida { get; set; }
    public int TotalPasajeros { get; set; }
    public int PasajerosAbordados { get; set; }
    public int PasajerosPendientes { get; set; }
    public int PasajerosNoAsistieron { get; set; }
    
    public List<PasajeroManifiestoDto> Pasajeros { get; set; } = new();
}

public class PasajeroManifiestoDto
{
    public int BoletoID { get; set; }
    public string CodigoQR { get; set; } = string.Empty;
    public string ClienteID { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteEmail { get; set; }
    public string? ClienteTelefono { get; set; }
    public string? AsientoAsignado { get; set; }
    public string EstadoBoleto { get; set; } = string.Empty;
    public string EstadoAbordaje { get; set; } = "Pendiente";
    public DateTime? FechaValidacion { get; set; }
    public string? ValidadoPor { get; set; }
}

