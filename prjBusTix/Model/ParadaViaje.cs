using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("ParadasViaje")]
    public class ParadaViaje
    {
        [Key]
        public int ParadaViajeID { get; set; }
        
        public int ViajeID { get; set; }
        
        public int? PlantillaParadaID { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string NombreParada { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(10,8)")]
        public decimal Latitud { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(11,8)")]
        public decimal Longitud { get; set; }
        
        [MaxLength(512)]
        public string? Direccion { get; set; }
        
        public int OrdenParada { get; set; }
        
        public DateTime? HoraEstimadaLlegada { get; set; }
        
        public int TiempoEsperaMinutos { get; set; } = 5;
        
        public bool EsActiva { get; set; } = true;
        
        // Relaciones
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje Viaje { get; set; } = null!;
        
        [ForeignKey(nameof(PlantillaParadaID))]
        public virtual PlantillaParada? PlantillaParada { get; set; }
        
        public virtual ICollection<Boleto> BoletosAbordaje { get; set; } = new List<Boleto>();
        public virtual ICollection<ManifiestoPasajero> ManifiestoPasajeros { get; set; } = new List<ManifiestoPasajero>();
    }
}

