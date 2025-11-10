namespace prjBusTix.Dto.Incidencias;

/// <summary>
/// DTO para las estadísticas de incidencias del dashboard
/// </summary>
public class EstadisticasIncidenciasDto
{
    /// <summary>
    /// Total de incidencias en el sistema
    /// </summary>
    public int TotalIncidencias { get; set; }
    
    /// <summary>
    /// Cantidad de incidencias abiertas/nuevas
    /// </summary>
    public int Abiertas { get; set; }
    
    /// <summary>
    /// Cantidad de incidencias en proceso
    /// </summary>
    public int EnProceso { get; set; }
    
    /// <summary>
    /// Cantidad de incidencias resueltas
    /// </summary>
    public int Resueltas { get; set; }
    
    /// <summary>
    /// Cantidad de incidencias cerradas
    /// </summary>
    public int Cerradas { get; set; }
    
    /// <summary>
    /// Cantidad de incidencias canceladas
    /// </summary>
    public int Canceladas { get; set; }
    
    /// <summary>
    /// Distribución por prioridad
    /// </summary>
    public List<ContadorPrioridad> PorPrioridad { get; set; } = new();
    
    /// <summary>
    /// Distribución por tipo de incidencia
    /// </summary>
    public List<ContadorTipo> PorTipo { get; set; } = new();
    
    /// <summary>
    /// Tiempo promedio de resolución en horas
    /// </summary>
    public double TiempoPromedioResolucionHoras { get; set; }
    
    /// <summary>
    /// Incidencias reportadas hoy
    /// </summary>
    public int ReportadasHoy { get; set; }
    
    /// <summary>
    /// Incidencias reportadas esta semana
    /// </summary>
    public int ReportadasEstaSemana { get; set; }
    
    /// <summary>
    /// Incidencias reportadas este mes
    /// </summary>
    public int ReportadasEsteMes { get; set; }
}

/// <summary>
/// Contador de incidencias por prioridad
/// </summary>
public class ContadorPrioridad
{
    public string Prioridad { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
}

/// <summary>
/// Contador de incidencias por tipo
/// </summary>
public class ContadorTipo
{
    public int TipoID { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
}

