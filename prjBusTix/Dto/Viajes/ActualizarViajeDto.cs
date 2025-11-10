using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Viajes;

public class ActualizarViajeDto
{
    public int? UnidadID { get; set; }
    
    [MaxLength(450)]
    public string? ChoferID { get; set; }
    
    public DateTime? FechaSalida { get; set; }
    
    public DateTime? FechaLlegadaEstimada { get; set; }
    
    [Range(0.01, 100000)]
    public decimal? PrecioBase { get; set; }
    
    [Range(0, 10000)]
    public decimal? CargoServicio { get; set; }
    
    public bool? VentasAbiertas { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? Estatus { get; set; }
}



