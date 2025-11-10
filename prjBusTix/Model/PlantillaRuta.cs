using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("PlantillasRutas")]
    public class PlantillaRuta
    {
        [Key]
        public int RutaID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string CodigoRuta { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string NombreRuta { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string CiudadOrigen { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string CiudadDestino { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(10,8)")]
        public decimal PuntoPartidaLat { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(11,8)")]
        public decimal PuntoPartidaLong { get; set; }
        
        [MaxLength(256)]
        public string? PuntoPartidaNombre { get; set; }
        
        [Column(TypeName = "decimal(10,8)")]
        public decimal? PuntoLlegadaLat { get; set; }
        
        [Column(TypeName = "decimal(11,8)")]
        public decimal? PuntoLlegadaLong { get; set; }
        
        [MaxLength(256)]
        public string? PuntoLlegadaNombre { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal? DistanciaKm { get; set; }
        
        public int? TiempoEstimadoMinutos { get; set; }
        
        public bool Activa { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        [MaxLength(450)]
        public string? CreadoPor { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(CreadoPor))]
        public virtual ClApplicationUser? Creador { get; set; }
        
        public virtual ICollection<PlantillaParada> Paradas { get; set; } = new List<PlantillaParada>();
        public virtual ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
    }
}

