namespace prjBusTix.Dto.Boletos;

/// <summary>
/// Respuesta del proceso de sincronización batch
/// </summary>
public class SincronizacionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalRecibidas { get; set; }
    public int Procesadas { get; set; }
    public int Fallidas { get; set; }
    public int Duplicadas { get; set; }
    public List<ErrorValidacionDto> Errores { get; set; } = new();
}

public class ErrorValidacionDto
{
    public string DeviceValidationId { get; set; } = string.Empty;
    public int? BoletoID { get; set; }
    public string Error { get; set; } = string.Empty;
}

