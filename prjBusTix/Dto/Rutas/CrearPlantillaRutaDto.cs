using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Rutas;

public class CrearPlantillaRutaDto
{
    [Required(ErrorMessage = "El código de ruta es requerido")]
    [MaxLength(50)]
    public string CodigoRuta { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre de ruta es requerido")]
    [MaxLength(200)]
    public string NombreRuta { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La ciudad de origen es requerida")]
    [MaxLength(100)]
    public string CiudadOrigen { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La ciudad de destino es requerida")]
    [MaxLength(100)]
    public string CiudadDestino { get; set; } = string.Empty;
    
    [Required]
    [Range(-90, 90)]
    public decimal PuntoPartidaLat { get; set; }
    
    [Required]
    [Range(-180, 180)]
    public decimal PuntoPartidaLong { get; set; }
    
    [MaxLength(256)]
    public string? PuntoPartidaNombre { get; set; }
    
    [Range(-90, 90)]
    public decimal? PuntoLlegadaLat { get; set; }
    
    [Range(-180, 180)]
    public decimal? PuntoLlegadaLong { get; set; }
    
    [MaxLength(256)]
    public string? PuntoLlegadaNombre { get; set; }
    
    [Range(0, 10000)]
    public decimal? DistanciaKm { get; set; }
    
    [Range(0, 1440)]
    public int? TiempoEstimadoMinutos { get; set; }
    
    public List<CrearParadaDto>? Paradas { get; set; }
}

public class CrearParadaDto
{
    [Required]
    [MaxLength(200)]
    public string NombreParada { get; set; } = string.Empty;
    
    [Required]
    [Range(-90, 90)]
    public decimal Latitud { get; set; }
    
    [Required]
    [Range(-180, 180)]
    public decimal Longitud { get; set; }
    
    [MaxLength(512)]
    public string? Direccion { get; set; }
    
    [Required]
    public int OrdenParada { get; set; }
    
    [Range(0, 120)]
    public int TiempoEsperaMinutos { get; set; } = 5;
}

