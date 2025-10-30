using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("ManifiestoPasajeros")]
    public class ManifiestoPasajero
    {
        [Key]
        public int ManifiestoID { get; set; }
        
        public int ViajeID { get; set; }
        
        public int BoletoID { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string? NumeroAsiento { get; set; }
        
        public int? ParadaAbordajeID { get; set; }
        
        public int EstatusAbordaje { get; set; } = 1; // Pendiente, Abordado, NoPresento
        
        public DateTime? FechaAbordaje { get; set; }
        
        public bool FueValidado { get; set; } = false;
        
        public DateTime? FechaValidacion { get; set; }
        
        [MaxLength(450)]
        public string? ValidadoPor { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje Viaje { get; set; } = null!;
        
        [ForeignKey(nameof(BoletoID))]
        public virtual Boleto Boleto { get; set; } = null!;
        
        [ForeignKey(nameof(EstatusAbordaje))]
        public virtual EstatusGeneral EstatusAbordajeNavigation { get; set; } = null!;
        
        [ForeignKey(nameof(ValidadoPor))]
        public virtual ClApplicationUser? Validador { get; set; }
        
        [ForeignKey(nameof(ParadaAbordajeID))]
        public virtual ParadaViaje? ParadaAbordaje { get; set; }
    }
}

