using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Boletos;

public class PasajeroDto
{
    [Required(ErrorMessage = "El nombre del pasajero es requerido")]
    [MaxLength(256)]
    public string NombrePasajero { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(256)]
    public string? EmailPasajero { get; set; }
    
    [MaxLength(50)]
    public string? TelefonoPasajero { get; set; }
}
