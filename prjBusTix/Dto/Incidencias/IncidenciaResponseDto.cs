namespace prjBusTix.Dto.Incidencias;

/// <summary>
/// DTO de respuesta con la información completa de una incidencia
/// Incluye datos relacionados (tipo, viaje, unidad, usuarios involucrados)
/// </summary>
public class IncidenciaResponseDto
{
    /// <summary>
    /// ID único de la incidencia
    /// </summary>
    public int IncidenciaID { get; set; }
    
    /// <summary>
    /// Código único de incidencia (formato: INC-YYYYMMDD-0001)
    /// </summary>
    public string CodigoIncidencia { get; set; } = string.Empty;
    
    // === Tipo de Incidencia ===
    public int TipoIncidenciaID { get; set; }
    public string TipoIncidenciaNombre { get; set; } = string.Empty;
    public string TipoIncidenciaCategoria { get; set; } = string.Empty;
    
    // === Viaje Relacionado (opcional) ===
    public int? ViajeID { get; set; }
    public string? ViajeCodigoViaje { get; set; }
    
    // === Unidad Relacionada (opcional) ===
    public int? UnidadID { get; set; }
    public string? UnidadPlacas { get; set; }
    
    // === Usuario que Reportó ===
    public string ReportadoPor { get; set; } = string.Empty;
    public string ReportadorNombre { get; set; } = string.Empty;
    public string? ReportadorEmail { get; set; }
    
    // === Detalles de la Incidencia ===
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    
    // === Fechas ===
    public DateTime FechaReporte { get; set; }
    public DateTime? FechaResolucion { get; set; }
    
    // === Estatus ===
    public int Estatus { get; set; }
    public string EstatusNombre { get; set; } = string.Empty;
    public string EstatusCodigo { get; set; } = string.Empty;
    
    // === Usuario Asignado (opcional) ===
    public string? AsignadoA { get; set; }
    public string? AsignadoNombre { get; set; }
    public string? AsignadoEmail { get; set; }
    
    // === Información Adicional ===
    /// <summary>
    /// Tiempo transcurrido desde el reporte
    /// </summary>
    public string TiempoTranscurrido { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si la incidencia está resuelta
    /// </summary>
    public bool EstaResuelta => Estatus == 3 || Estatus == 4;
    
    /// <summary>
    /// Días desde el reporte
    /// </summary>
    public int DiasDesdeReporte => (DateTime.Now - FechaReporte).Days;
}
