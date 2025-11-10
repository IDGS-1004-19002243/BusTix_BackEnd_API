using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Boletos;

public class ValidarBoletoDto
{
    [Required]
    [MaxLength(500)]
    public string CodigoQR { get; set; } = string.Empty;
    
    public decimal? Latitud { get; set; }
    
    public decimal? Longitud { get; set; }
    
    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}

public class ValidarBoletoResponseDto
{
    public bool EsValido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int? BoletoID { get; set; }
    public string? NombrePasajero { get; set; }
    public string? NumeroAsiento { get; set; }
    public string? CodigoViaje { get; set; }
    public string? CiudadOrigen { get; set; }
    public string? CiudadDestino { get; set; }
    public DateTime? FechaSalida { get; set; }
    public DateTime? FechaValidacion { get; set; }
    public string? ValidadoPor { get; set; }
}

