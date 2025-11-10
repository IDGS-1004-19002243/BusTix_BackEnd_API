using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Unidades")]
    public class Unidad
    {
        [Key]
        public int UnidadID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string NumeroEconomico { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Placas { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Marca { get; set; }
        
        [MaxLength(100)]
        public string? Modelo { get; set; }
        
        public int? Año { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string TipoUnidad { get; set; } = string.Empty;
        
        public int CapacidadAsientos { get; set; }
        
        public bool TieneClimatizacion { get; set; } = true;
        
        public bool TieneBaño { get; set; } = false;
        
        public bool TieneWifi { get; set; } = false;
        
        [MaxLength(512)]
        public string? UrlFoto { get; set; }
        
        public int Estatus { get; set; } = 1;
        
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        
        // Relaciones
        [ForeignKey(nameof(Estatus))]
        public virtual EstatusGeneral EstatusNavigation { get; set; } = null!;
        
        public virtual ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
        public virtual ICollection<Incidencia> Incidencias { get; set; } = new List<Incidencia>();
    }
}

