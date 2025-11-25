namespace prjBusTix.Dto.Viajes
{
    /// <summary>
    /// DTO para crear/actualizar precios por parada
    /// </summary>
    public class ConfigurarPrecioParadaDto
    {
        public int ParadaViajeID { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal CargoServicio { get; set; } = 0;
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// DTO para configurar precios de múltiples paradas a la vez
    /// </summary>
    public class ConfigurarPreciosViajeDto
    {
        public int ViajeID { get; set; }
        public List<ConfigurarPrecioParadaDto> Precios { get; set; } = new();
    }

    /// <summary>
    /// DTO de respuesta con información de precio por parada
    /// </summary>
    public class PrecioParadaResponseDto
    {
        public int PrecioParadaID { get; set; }
        public int ViajeID { get; set; }
        public int ParadaViajeID { get; set; }
        public string NombreParada { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public decimal CargoServicio { get; set; }
        public decimal PrecioTotal { get; set; }
        public bool EsActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? CreadoPor { get; set; }
        public string? NombreCreador { get; set; }
        public string? Observaciones { get; set; }
    }
}

