using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("RegistroValidacion")]
    public class RegistroValidacion
    {
        [Key]
        public int ValidacionID { get; set; }
        
        public int BoletoID { get; set; }
        
        public int ViajeID { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string ValidadoPor { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? CodigoQREscaneado { get; set; }
        
        [MaxLength(50)]
        public string? ResultadoValidacion { get; set; } // Exitosa, Duplicada, Invalida
        
        public DateTime FechaHoraValidacion { get; set; } = DateTime.Now;
        
        public bool ModoOffline { get; set; } = false;
        
        public DateTime? FechaSincronizacion { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(BoletoID))]
        public virtual Boleto Boleto { get; set; } = null!;
        
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje Viaje { get; set; } = null!;
        
        [ForeignKey(nameof(ValidadoPor))]
        public virtual ClApplicationUser Validador { get; set; } = null!;
    }
}

