namespace prjBusTix.Dto.Viajes
{
    /// <summary>
    /// DTO con información detallada del viaje para el cliente
    /// </summary>
    public class ViajeDetalleClienteDto
    {
        public int ViajeID { get; set; }
        public string CodigoViaje { get; set; } = string.Empty;
        public string TipoViaje { get; set; } = string.Empty;
        
        // Información del evento
        public int EventoID { get; set; }
        public string EventoNombre { get; set; } = string.Empty;
        public string EventoDescripcion { get; set; } = string.Empty;
        public DateTime EventoFecha { get; set; }
        public string EventoRecinto { get; set; } = string.Empty;
        public string EventoCiudad { get; set; } = string.Empty;
        public string? EventoUrlImagen { get; set; }
        
        // Información de la ruta
        public string RutaNombre { get; set; } = string.Empty;
        public string CiudadOrigen { get; set; } = string.Empty;
        public string CiudadDestino { get; set; } = string.Empty;
        
        // Información del viaje
        public DateTime FechaSalida { get; set; }
        public DateTime? FechaLlegadaEstimada { get; set; }
        public int DuracionEstimadaHoras { get; set; }
        
        // Disponibilidad
        public int CupoTotal { get; set; }
        public int AsientosDisponibles { get; set; }
        public int AsientosVendidos { get; set; }
        public decimal PorcentajeOcupacion => CupoTotal > 0 ? (decimal)AsientosVendidos * 100 / CupoTotal : 0;
        public bool VentasAbiertas { get; set; }
        
        // Precios
        public decimal PrecioBase { get; set; }
        public decimal CargoServicio { get; set; }
        public decimal PrecioDesde { get; set; } // Precio más bajo disponible
        public decimal PrecioHasta { get; set; } // Precio más alto
        
        // Unidad y chofer
        public string? UnidadModelo { get; set; }
        public string? UnidadPlacas { get; set; }
        public int? CapacidadUnidad { get; set; }
        public string? ChoferNombre { get; set; }
        
        // Paradas disponibles
        public List<ParadaConPrecioDto> Paradas { get; set; } = new();
        
        // Información adicional
        public string? Observaciones { get; set; }
        public bool TieneServicioWifi { get; set; }
        public bool TieneAireAcondicionado { get; set; }
        public bool TieneBaño { get; set; }
    }

    /// <summary>
    /// DTO de parada con su precio específico
    /// </summary>
    public class ParadaConPrecioDto
    {
        public int ParadaViajeID { get; set; }
        public string NombreParada { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int OrdenParada { get; set; }
        public DateTime? HoraEstimadaLlegada { get; set; }
        public int TiempoEsperaMinutos { get; set; }
        
        // Precio específico de esta parada
        public decimal PrecioBase { get; set; }
        public decimal CargoServicio { get; set; }
        public decimal PrecioTotal { get; set; }
        public decimal IVA { get; set; }
        public decimal TotalAPagar { get; set; }
        
        // Disponibilidad en esta parada
        public int AsientosDisponibles { get; set; }
        public bool TieneDisponibilidad => AsientosDisponibles > 0;
    }
}

