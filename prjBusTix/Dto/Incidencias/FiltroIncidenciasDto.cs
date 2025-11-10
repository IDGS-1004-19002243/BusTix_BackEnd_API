namespace prjBusTix.Dto.Incidencias;

/// <summary>
/// DTO para filtrar y paginar incidencias en el dashboard de administradores
/// </summary>
public class FiltroIncidenciasDto
{
    /// <summary>
    /// Filtrar por estatus específico
    /// 1=Abierta, 2=En Proceso, 3=Resuelta, 4=Cerrada, 5=Cancelada
    /// </summary>
    public int? Estatus { get; set; }
    
    /// <summary>
    /// Filtrar por prioridad: Baja, Media, Alta, Crítica
    /// </summary>
    public string? Prioridad { get; set; }
    
    /// <summary>
    /// Filtrar por viaje específico
    /// </summary>
    public int? ViajeID { get; set; }
    
    /// <summary>
    /// Filtrar por tipo de incidencia
    /// </summary>
    public int? TipoIncidenciaID { get; set; }
    
    /// <summary>
    /// Filtrar por usuario asignado
    /// </summary>
    public string? AsignadoA { get; set; }
    
    /// <summary>
    /// Filtrar por usuario que reportó
    /// </summary>
    public string? ReportadoPor { get; set; }
    
    /// <summary>
    /// Filtrar por rango de fechas - desde
    /// </summary>
    public DateTime? FechaDesde { get; set; }
    
    /// <summary>
    /// Filtrar por rango de fechas - hasta
    /// </summary>
    public DateTime? FechaHasta { get; set; }
    
    /// <summary>
    /// Búsqueda por texto en título o descripción
    /// </summary>
    public string? TextoBusqueda { get; set; }
    
    /// <summary>
    /// Número de página para paginación (inicia en 1)
    /// </summary>
    public int Pagina { get; set; } = 1;
    
    /// <summary>
    /// Cantidad de registros por página
    /// </summary>
    public int TamanoPagina { get; set; } = 20;
    
    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string? OrdenarPor { get; set; } = "FechaReporte";
    
    /// <summary>
    /// Dirección del ordenamiento: asc o desc
    /// </summary>
    public string? DireccionOrden { get; set; } = "desc";
}
