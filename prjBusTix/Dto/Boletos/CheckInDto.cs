namespace prjBusTix.Dto.Boletos;

public class CheckInDto
{
    public string? Observaciones { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}

public class CheckInResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int BoletoID { get; set; }
    public DateTime? FechaCheckIn { get; set; }
    public string? ClienteNombre { get; set; }
    public string? NumeroAsiento { get; set; }
}
