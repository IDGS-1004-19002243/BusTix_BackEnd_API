namespace prjBusTix.Dto.Rutas;

public class PlantillaRutaResponseDto
{
    public int RutaID { get; set; }
    public string CodigoRuta { get; set; } = string.Empty;
    public string NombreRuta { get; set; } = string.Empty;
    public string CiudadOrigen { get; set; } = string.Empty;
    public string CiudadDestino { get; set; } = string.Empty;
    public decimal PuntoPartidaLat { get; set; }
    public decimal PuntoPartidaLong { get; set; }
    public string? PuntoPartidaNombre { get; set; }
    public decimal? PuntoLlegadaLat { get; set; }
    public decimal? PuntoLlegadaLong { get; set; }
    public string? PuntoLlegadaNombre { get; set; }
    public decimal? DistanciaKm { get; set; }
    public int? TiempoEstimadoMinutos { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int TotalParadas { get; set; }
    public int TotalViajes { get; set; }
}

