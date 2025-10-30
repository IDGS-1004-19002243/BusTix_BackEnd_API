using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("ViajesStaff")]
    public class ViajeStaff
    {
        [Key]
        public int AsignacionID { get; set; }
        
        public int ViajeID { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string StaffID { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string RolEnViaje { get; set; } = string.Empty; // Validador, Supervisor, Auxiliar
        
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;
        
        // Relaciones
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje Viaje { get; set; } = null!;
        
        [ForeignKey(nameof(StaffID))]
        public virtual ClApplicationUser Staff { get; set; } = null!;
    }
}

