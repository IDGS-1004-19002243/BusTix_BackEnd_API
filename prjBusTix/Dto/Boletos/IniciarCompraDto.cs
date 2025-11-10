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
    [MaxLength(256)]
    public string NombrePasajero { get; set; } = string.Empty;
    
    [EmailAddress]
    [MaxLength(256)]
    public string? EmailPasajero { get; set; }
    
    [MaxLength(50)]
    public string? TelefonoPasajero { get; set; }
    
    [MaxLength(10)]
    public string? NumeroAsiento { get; set; }
    
    public int? ParadaAbordajeID { get; set; }
    
    public int? CuponID { get; set; }
}

