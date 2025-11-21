using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Boletos;

/// <summary>
/// DTO para iniciar el proceso de compra de un boleto
/// </summary>
public class IniciarCompraDto
{
    [Required]
    public int ViajeID { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un pasajero")]
    public List<PasajeroDto> Pasajeros { get; set; } = new();
    
    public int? ParadaAbordajeID { get; set; }
    
    public int? CuponID { get; set; }
}

