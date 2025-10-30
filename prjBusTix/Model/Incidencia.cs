using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Incidencias")]
    public class Incidencia
    {
        [Key]
        public int IncidenciaID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string CodigoIncidencia { get; set; } = string.Empty;
        
        public int TipoIncidenciaID { get; set; }
        
        public int? ViajeID { get; set; }
        
        public int? UnidadID { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string ReportadoPor { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(256)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Descripcion { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Prioridad { get; set; } = string.Empty;
        
        public DateTime FechaReporte { get; set; } = DateTime.Now;
        
        public int Estatus { get; set; } = 1;
        
        [MaxLength(450)]
        public string? AsignadoA { get; set; }
        
        public DateTime? FechaResolucion { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(TipoIncidenciaID))]
        public virtual TipoIncidencia TipoIncidencia { get; set; } = null!;
        
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje? Viaje { get; set; }
        
        [ForeignKey(nameof(UnidadID))]
        public virtual Unidad? Unidad { get; set; }
        
        [ForeignKey(nameof(ReportadoPor))]
        public virtual ClApplicationUser Reportador { get; set; } = null!;
        
        [ForeignKey(nameof(AsignadoA))]
        public virtual ClApplicationUser? Asignado { get; set; }
        
        [ForeignKey(nameof(Estatus))]
        public virtual EstatusGeneral EstatusNavigation { get; set; } = null!;
    }
}

