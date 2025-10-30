using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("PagosBoletos")]
    public class PagoBoleto
    {
        [Key]
        public int PagoBoletoID { get; set; }
        
        public int PagoID { get; set; }
        
        public int BoletoID { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoAsignado { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(PagoID))]
        public virtual Pago Pago { get; set; } = null!;
        
        [ForeignKey(nameof(BoletoID))]
        public virtual Boleto Boleto { get; set; } = null!;
    }
}

