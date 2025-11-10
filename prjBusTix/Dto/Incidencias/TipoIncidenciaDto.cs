namespace prjBusTix.Dto.Incidencias;

/// <summary>
/// DTO para respuesta de tipo de incidencia (catálogo)
/// </summary>
public class TipoIncidenciaDto
{
    public int TipoIncidenciaID { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? Prioridad { get; set; }
    public bool EsActivo { get; set; }
}

