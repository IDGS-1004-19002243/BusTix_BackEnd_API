using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Eventos;

public class CrearEventoDto
{
    [Required(ErrorMessage = "El nombre del evento es requerido")]
    [MaxLength(256)]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Descripcion { get; set; }
    
    [MaxLength(50)]
    public string? TipoEvento { get; set; }
    
    [Required(ErrorMessage = "La fecha del evento es requerida")]
    public DateTime Fecha { get; set; }
    
    public TimeSpan? HoraInicio { get; set; }
    
    [MaxLength(256)]
    public string? Recinto { get; set; }
    
    [MaxLength(512)]
    public string? Direccion { get; set; }
    
    [MaxLength(100)]
    public string? Ciudad { get; set; }
    
    [MaxLength(100)]
    public string? Estado { get; set; }
    
    [Range(-90, 90)]
    public decimal? UbicacionLat { get; set; }
    
    [Range(-180, 180)]
    public decimal? UbicacionLong { get; set; }
    
    [MaxLength(512)]
    [Url]
    public string? UrlImagen { get; set; }
}

