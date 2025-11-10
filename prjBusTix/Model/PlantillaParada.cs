using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("PlantillasParadas")]
    public class PlantillaParada
    {
        [Key]
        public int ParadaID { get; set; }
        
        public int PlantillaRutaID { get; set; }
        
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
        
        public int TiempoEsperaMinutos { get; set; } = 5;
        
        public bool EsActiva { get; set; } = true;
        
        // Relaciones
        [ForeignKey(nameof(PlantillaRutaID))]
        public virtual PlantillaRuta PlantillaRuta { get; set; } = null!;
        
        public virtual ICollection<ParadaViaje> ParadasViaje { get; set; } = new List<ParadaViaje>();
    }
}

