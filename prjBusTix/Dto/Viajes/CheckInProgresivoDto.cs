namespace prjBusTix.Dto.Viajes
{
    /// <summary>
    /// DTO para que el chofer confirme llegada a una parada
    /// </summary>
    public class ConfirmarLlegadaParadaDto
    {
        public int ParadaViajeID { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// DTO para que el staff inicie validación de pasajeros
    /// </summary>
    public class IniciarValidacionParadaDto
    {
        public int ParadaViajeID { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// DTO para finalizar validación de una parada
    /// </summary>
    public class FinalizarValidacionParadaDto
    {
        public int ParadaViajeID { get; set; }
        public int TotalAbordados { get; set; }
        public int TotalNoShow { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// DTO de respuesta con el estado de una parada
    /// </summary>
    public class EstadoParadaResponseDto
    {
        public int EstadoParadaViajeID { get; set; }
        public int ViajeID { get; set; }
        public string CodigoViaje { get; set; } = string.Empty;
        public int ParadaViajeID { get; set; }
        public string NombreParada { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int OrdenParada { get; set; }
        public DateTime? HoraEstimadaLlegada { get; set; }
        
        // Estado actual
        public string Estado { get; set; } = "Pendiente";
        
        // Información del chofer
        public DateTime? FechaHoraLlegadaChofer { get; set; }
        public string? ChoferID { get; set; }
        public string? NombreChofer { get; set; }
        public decimal? LatitudConfirmacion { get; set; }
        public decimal? LongitudConfirmacion { get; set; }
        
        // Información del staff
        public DateTime? FechaHoraInicioValidacion { get; set; }
        public DateTime? FechaHoraFinalizacionValidacion { get; set; }
        public string? StaffID { get; set; }
        public string? NombreStaff { get; set; }
        
        // Estadísticas
        public int TotalPasajerosEsperados { get; set; }
        public int TotalPasajerosAbordados { get; set; }
        public int TotalPasajerosNoShow { get; set; }
        public int PasajerosPorValidar => TotalPasajerosEsperados - TotalPasajerosAbordados - TotalPasajerosNoShow;
        
        public bool TuvoIncidencia { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// DTO para obtener el progreso completo de un viaje
    /// </summary>
    public class ProgresoViajeDto
    {
        public int ViajeID { get; set; }
        public string CodigoViaje { get; set; } = string.Empty;
        public string EstadoGeneral { get; set; } = "Pendiente"; // Pendiente, EnRuta, Completado
        public DateTime FechaSalida { get; set; }
        public int TotalParadas { get; set; }
        public int ParadasCompletadas { get; set; }
        public int ParadaActual { get; set; }
        public List<EstadoParadaResponseDto> Paradas { get; set; } = new();
        
        // Estadísticas globales
        public int TotalPasajerosViaje { get; set; }
        public int TotalAbordados { get; set; }
        public int TotalNoShow { get; set; }
        public int TotalPendientes => TotalPasajerosViaje - TotalAbordados - TotalNoShow;
        public decimal PorcentajeAvance => TotalParadas > 0 ? (decimal)ParadasCompletadas * 100m / TotalParadas : 0;
    }
}

