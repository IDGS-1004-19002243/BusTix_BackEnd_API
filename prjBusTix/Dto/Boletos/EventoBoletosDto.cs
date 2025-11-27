namespace prjBusTix.Dto.Boletos;

public class EventoBoletosDto
{
    public int EventoID { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public string? ImagenEvento { get; set; }
    public string? UbicacionEvento { get; set; }
    public DateTime FechaEvento { get; set; }
    
    public List<TransaccionBoletosDto> Transacciones { get; set; } = new();
}
