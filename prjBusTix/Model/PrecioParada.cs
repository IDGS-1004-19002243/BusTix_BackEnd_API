using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    /// <summary>
    /// Define precios personalizados por parada de viaje
    /// Permite configurar tarifas diferentes según el punto de abordaje
    /// </summary>
    [Table("PreciosParada")]
    public class PrecioParada
    {
        [Key]
        public int PrecioParadaID { get; set; }
        
        /// <summary>
        /// Viaje al que aplica este precio
        /// </summary>
        public int ViajeID { get; set; }
        
        /// <summary>
        /// Parada específica del viaje
        /// </summary>
        public int ParadaViajeID { get; set; }
        
        /// <summary>
        /// Precio base para abordar en esta parada
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioBase { get; set; }
        
        /// <summary>
        /// Cargo por servicio adicional (puede ser 0)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal CargoServicio { get; set; } = 0;
        
        /// <summary>
        /// Precio total calculado (PrecioBase + CargoServicio)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioTotal { get; set; }
        
        /// <summary>
        /// Indica si este precio está activo
        /// </summary>
        public bool EsActivo { get; set; } = true;
        
        /// <summary>
        /// Fecha de creación del precio
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Usuario que creó este precio
        /// </summary>
        [MaxLength(450)]
        public string? CreadoPor { get; set; }
        
        /// <summary>
        /// Notas adicionales sobre este precio
        /// </summary>
        [MaxLength(500)]
        public string? Observaciones { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje Viaje { get; set; } = null!;
        
        [ForeignKey(nameof(ParadaViajeID))]
        public virtual ParadaViaje ParadaViaje { get; set; } = null!;
        
        [ForeignKey(nameof(CreadoPor))]
        public virtual ClApplicationUser? Creador { get; set; }
    }
}

