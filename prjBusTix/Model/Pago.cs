using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Pagos")]
    public class Pago
    {
        [Key]
        public int PagoID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string CodigoPago { get; set; } = string.Empty;
        
        [MaxLength(256)]
        public string? TransaccionID { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UsuarioID { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string MetodoPago { get; set; } = string.Empty; // Tarjeta, Efectivo
        
        [MaxLength(100)]
        public string? Proveedor { get; set; } // Stripe, MercadoPago
        
        public DateTime FechaPago { get; set; } = DateTime.Now;
        
        public int Estatus { get; set; } = 1;
        
        // Relaciones
        [ForeignKey(nameof(UsuarioID))]
        public virtual ClApplicationUser Usuario { get; set; } = null!;
        
        [ForeignKey(nameof(Estatus))]
        public virtual EstatusGeneral EstatusNavigation { get; set; } = null!;
        
        public virtual ICollection<PagoBoleto> PagosBoletos { get; set; } = new List<PagoBoleto>();
    }
}

