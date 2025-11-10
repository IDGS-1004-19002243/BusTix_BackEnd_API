namespace prjBusTix.Dto.Eventos;

public class EventoResponseDto
{
    public int EventoID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? TipoEvento { get; set; }
    public DateTime Fecha { get; set; }
    public TimeSpan? HoraInicio { get; set; }
    public string? Recinto { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public decimal? UbicacionLat { get; set; }
    public decimal? UbicacionLong { get; set; }
    public string? UrlImagen { get; set; }
    public int Estatus { get; set; }
    public string? EstatusNombre { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
    public int TotalViajes { get; set; }
}

