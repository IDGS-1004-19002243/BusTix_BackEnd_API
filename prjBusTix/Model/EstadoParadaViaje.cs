using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    /// <summary>
    /// Registra el estado de check-in en cada parada del viaje
    /// Permite el flujo: Chofer confirma llegada → Staff valida pasajeros
    /// </summary>
    [Table("EstadoParadaViaje")]
    public class EstadoParadaViaje
    {
        [Key]
        public int EstadoParadaViajeID { get; set; }
        
        /// <summary>
        /// Viaje al que pertenece esta parada
        /// </summary>
        public int ViajeID { get; set; }
        
        /// <summary>
        /// Parada específica del viaje
        /// </summary>
        public int ParadaViajeID { get; set; }
        
        /// <summary>
        /// Estado actual de la parada: Pendiente, EnCamino, Llegado, Validando, Completado
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Pendiente";
        
        /// <summary>
        /// Fecha/hora en que el chofer confirmó llegada a la parada
        /// </summary>
        public DateTime? FechaHoraLlegadaChofer { get; set; }
        
        /// <summary>
        /// ID del chofer que confirmó la llegada
        /// </summary>
        [MaxLength(450)]
        public string? ConfirmadoPorChofer { get; set; }
        
        /// <summary>
        /// Ubicación GPS donde el chofer confirmó (latitud)
        /// </summary>
        [Column(TypeName = "decimal(10,8)")]
        public decimal? LatitudConfirmacion { get; set; }
        
        /// <summary>
        /// Ubicación GPS donde el chofer confirmó (longitud)
        /// </summary>
        [Column(TypeName = "decimal(11,8)")]
        public decimal? LongitudConfirmacion { get; set; }
        
        /// <summary>
        /// Fecha/hora en que el staff inició validación de pasajeros
        /// </summary>
        public DateTime? FechaHoraInicioValidacion { get; set; }
        
        /// <summary>
        /// Fecha/hora en que se completó la validación de todos los pasajeros
        /// </summary>
        public DateTime? FechaHoraFinalizacionValidacion { get; set; }
        
        /// <summary>
        /// ID del staff que validó (puede ser múltiple, guardar el principal)
        /// </summary>
        [MaxLength(450)]
        public string? ValidadoPorStaff { get; set; }
        
        /// <summary>
        /// Total de pasajeros esperados en esta parada
        /// </summary>
        public int TotalPasajerosEsperados { get; set; }
        
        /// <summary>
        /// Total de pasajeros que abordaron efectivamente
        /// </summary>
        public int TotalPasajerosAbordados { get; set; }
        
        /// <summary>
        /// Total de pasajeros que NO abordaron (no show)
        /// </summary>
        public int TotalPasajerosNoShow { get; set; }
        
        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        [MaxLength(1000)]
        public string? Observaciones { get; set; }
        
        /// <summary>
        /// Indica si hubo alguna incidencia en esta parada
        /// </summary>
        public bool TuvoIncidencia { get; set; } = false;
        
        // Relaciones
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje Viaje { get; set; } = null!;
        
        [ForeignKey(nameof(ParadaViajeID))]
        public virtual ParadaViaje ParadaViaje { get; set; } = null!;
        
        [ForeignKey(nameof(ConfirmadoPorChofer))]
        public virtual ClApplicationUser? Chofer { get; set; }
        
        [ForeignKey(nameof(ValidadoPorStaff))]
        public virtual ClApplicationUser? Staff { get; set; }
    }
}

