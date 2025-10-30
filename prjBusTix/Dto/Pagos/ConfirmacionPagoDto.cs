using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Pagos;

/// <summary>
/// DTO para confirmar un pago desde webhook de pasarela
/// </summary>
public class ConfirmacionPagoDto
{
    [Required]
    public string TransaccionID { get; set; } = string.Empty;
    
    [Required]
    public string CodigoPago { get; set; } = string.Empty;
    
    [Required]
    public string Estado { get; set; } = string.Empty; // "approved", "pending", "rejected"
    
    public string? Proveedor { get; set; } // "Stripe", "MercadoPago"
    
    public decimal? MontoConfirmado { get; set; }
    
    public Dictionary<string, string>? MetadataAdicional { get; set; }
}

