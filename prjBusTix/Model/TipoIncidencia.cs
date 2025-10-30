using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("TipoIncidencia")]
    public class TipoIncidencia
    {
        [Key]
        public int TipoIncidenciaID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Categoria { get; set; }
        
        [MaxLength(50)]
        public string? Prioridad { get; set; }
        
        public bool EsActivo { get; set; } = true;
        
        // Relaciones
        public virtual ICollection<Incidencia> Incidencias { get; set; } = new List<Incidencia>();
    }
}

