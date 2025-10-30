namespace prjBusTix.Dto.Boletos;

/// <summary>
/// DTO con la información completa del boleto después de la compra
/// </summary>
public class BoletoResponseDto
{
    public int BoletoID { get; set; }
    public string CodigoBoleto { get; set; } = string.Empty;
    public string CodigoQR { get; set; } = string.Empty;
    
    // Información del Viaje
    public int ViajeID { get; set; }
    public string CodigoViaje { get; set; } = string.Empty;
    public string CiudadOrigen { get; set; } = string.Empty;
    public string CiudadDestino { get; set; } = string.Empty;
    public DateTime FechaSalida { get; set; }
    public string? NumeroAsiento { get; set; }
    
    // Información del Pasajero
    public string NombrePasajero { get; set; } = string.Empty;
    public string? EmailPasajero { get; set; }
    public string? TelefonoPasajero { get; set; }
    
    // Información de Precios
    public decimal PrecioBase { get; set; }
    public decimal Descuento { get; set; }
    public decimal CargoServicio { get; set; }
    public decimal IVA { get; set; }
    public decimal PrecioTotal { get; set; }
    
    // Estado
    public int Estatus { get; set; }
    public string EstatusNombre { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public DateTime? FechaValidacion { get; set; }
    
    // Parada
    public string? ParadaAbordaje { get; set; }
    public DateTime? HoraEstimadaAbordaje { get; set; }
}

