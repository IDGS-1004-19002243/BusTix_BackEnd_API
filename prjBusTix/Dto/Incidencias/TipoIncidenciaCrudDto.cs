namespace prjBusTix.Dto.Incidencias;

/// <summary>
/// DTO para crear un nuevo tipo de incidencia
/// </summary>
public class CrearTipoIncidenciaDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? Prioridad { get; set; }
}

/// <summary>
/// DTO para actualizar un tipo de incidencia existente
/// </summary>
public class ActualizarTipoIncidenciaDto
{
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Categoria { get; set; }
    public string? Prioridad { get; set; }
    public bool? EsActivo { get; set; }
}
