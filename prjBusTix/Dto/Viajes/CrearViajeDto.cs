using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Viajes;

public class CrearViajeDto
{
    [Required(ErrorMessage = "El evento es requerido")]
    public int EventoID { get; set; }
    
    [Required(ErrorMessage = "La plantilla de ruta es requerida")]
    public int PlantillaRutaID { get; set; }
    
    public int? UnidadID { get; set; }
    
    [MaxLength(450)]
    public string? ChoferID { get; set; }
    
    [Required(ErrorMessage = "El tipo de viaje es requerido")]
    [MaxLength(50)]
    public string TipoViaje { get; set; } = string.Empty; // "Ida" o "Regreso"
    
    [Required(ErrorMessage = "La fecha de salida es requerida")]
    public DateTime FechaSalida { get; set; }
    
    public DateTime? FechaLlegadaEstimada { get; set; }
    
    [Required(ErrorMessage = "El cupo total es requerido")]
    [Range(1, 100, ErrorMessage = "El cupo debe estar entre 1 y 100")]
    public int CupoTotal { get; set; }
    
    [Required(ErrorMessage = "El precio base es requerido")]
    [Range(0.01, 100000, ErrorMessage = "El precio base debe ser mayor a 0")]
    public decimal PrecioBase { get; set; }
    
    [Range(0, 10000)]
    public decimal CargoServicio { get; set; } = 0;
    
    public bool VentasAbiertas { get; set; } = true;
}

