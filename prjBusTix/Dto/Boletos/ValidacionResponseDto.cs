namespace prjBusTix.Dto.Boletos;

/// <summary>
/// Respuesta de validación de boleto
/// </summary>
public class ValidacionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ValidacionID { get; set; }
    public string? Resultado { get; set; }
    public DateTime? FechaHoraValidacion { get; set; }
    
    // Información del boleto validado
    public int BoletoID { get; set; }
    public string? ClienteNombre { get; set; }
    public string? AsientoAsignado { get; set; }
    public string? EstadoBoleto { get; set; }
}

