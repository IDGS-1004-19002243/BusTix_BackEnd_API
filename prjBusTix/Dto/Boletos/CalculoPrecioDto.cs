namespace prjBusTix.Dto.Boletos;

/// <summary>
/// DTO con el cálculo del precio y detalles para confirmar la compra
/// </summary>
public class CalculoPrecioDto
{
    public int ViajeID { get; set; }
    public string CodigoViaje { get; set; } = string.Empty;
    public string CiudadOrigen { get; set; } = string.Empty;
    public string CiudadDestino { get; set; } = string.Empty;
    public DateTime FechaSalida { get; set; }
    
    public decimal PrecioBase { get; set; }
    public decimal CargoServicio { get; set; }
    public decimal Descuento { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    public decimal Subtotal { get; set; }
    public decimal IVA { get; set; }
    public decimal PrecioTotal { get; set; }
    
    public string? CuponAplicado { get; set; }
    public string? ParadaAbordaje { get; set; }
    
    public bool VentasAbiertas { get; set; }
    public int AsientosDisponibles { get; set; }
}

