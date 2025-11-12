namespace prjBusTix.Dto.Viajes;

public class ConflictoDto
{
    public int ViajeID { get; set; }
    public string CodigoViaje { get; set; } = string.Empty;
    public DateTime FechaSalida { get; set; }
    public DateTime? FechaLlegadaEstimada { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public string RutaNombre { get; set; } = string.Empty;
}

public class DisponibilidadResponseDto
{
    public bool EstaDisponible { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public List<ConflictoDto> Conflictos { get; set; } = new();
}
