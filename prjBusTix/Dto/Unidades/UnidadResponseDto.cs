public class UnidadResponseDto
{
    public int Id { get; set; }
    public required string NumeroEconomico { get; set; }
    public required string Placas { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int? Año { get; set; }
    public required string TipoUnidad { get; set; }
    public int CapacidadAsientos { get; set; }
    public bool TieneClimatizacion { get; set; }
    public bool TieneBaño { get; set; }
    public bool TieneWifi { get; set; }
    public string? UrlFoto { get; set; }
    public int Estatus { get; set; }
    public DateTime FechaAlta { get; set; }
}