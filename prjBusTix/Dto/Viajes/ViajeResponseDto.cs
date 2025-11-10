namespace prjBusTix.Dto.Viajes;

public class ViajeResponseDto
{
    public int ViajeID { get; set; }
    public string CodigoViaje { get; set; } = string.Empty;
    public string TipoViaje { get; set; } = string.Empty;
    
    // Información del Evento
    public int EventoID { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public DateTime EventoFecha { get; set; }
    
    // Información de la Ruta
    public int PlantillaRutaID { get; set; }
    public string RutaNombre { get; set; } = string.Empty;
    public string CiudadOrigen { get; set; } = string.Empty;
    public string CiudadDestino { get; set; } = string.Empty;
    
    // Información de la Unidad
    public int? UnidadID { get; set; }
    public string? UnidadPlacas { get; set; }
    public string? UnidadModelo { get; set; }
    
    // Información del Chofer
    public string? ChoferID { get; set; }
    public string? ChoferNombre { get; set; }
    
    // Detalles del Viaje
    public DateTime FechaSalida { get; set; }
    public DateTime? FechaLlegadaEstimada { get; set; }
    public int CupoTotal { get; set; }
    public int AsientosDisponibles { get; set; }
    public int AsientosVendidos { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal CargoServicio { get; set; }
    public bool VentasAbiertas { get; set; }
    
    public int Estatus { get; set; }
    public string? EstatusNombre { get; set; }
    public DateTime FechaCreacion { get; set; }
    
    public int TotalParadas { get; set; }
    public int TotalStaff { get; set; }
    public int TotalIncidencias { get; set; }
}

