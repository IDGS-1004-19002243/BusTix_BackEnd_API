namespace prjBusTix.Dto.Boletos;

public class BoletoDetalleViajeDto
{
    public int ViajeID { get; set; }
    public string CodigoViaje { get; set; } = string.Empty;
    public string CiudadOrigen { get; set; } = string.Empty;
    public string CiudadDestino { get; set; } = string.Empty;
    public DateTime FechaSalida { get; set; }
    public DateTime? FechaLlegadaEstimada { get; set; }
    
    // Información de la Unidad
    public string? UnidadPlacas { get; set; }
    public string? UnidadNumeroEconomico { get; set; }
}
