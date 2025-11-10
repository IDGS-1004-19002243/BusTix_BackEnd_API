namespace prjBusTix.Dto.Incidencias;

/// <summary>
/// DTO simplificado para listados de incidencias (para mejorar performance en listas)
/// </summary>
public class IncidenciaListaDto
{
    public int IncidenciaID { get; set; }
    public string CodigoIncidencia { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string TipoIncidenciaNombre { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public DateTime FechaReporte { get; set; }
    public int Estatus { get; set; }
    public string EstatusNombre { get; set; } = string.Empty;
    public string ReportadorNombre { get; set; } = string.Empty;
    public string? AsignadoNombre { get; set; }
    public int? ViajeID { get; set; }
    public string? ViajeCodigoViaje { get; set; }
}

