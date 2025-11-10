using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Cupones")]
    public class Cupon
    {
        [Key]
        public int CuponID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Descripcion { get; set; }
        
        [MaxLength(50)]
        public string? TipoDescuento { get; set; } // Porcentaje, MontoFijo
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorDescuento { get; set; }
        
        public int? UsosMaximos { get; set; }
        
        public int UsosRealizados { get; set; } = 0;
        
        public DateTime? FechaInicio { get; set; }
        
        public DateTime? FechaExpiracion { get; set; }
        
        public bool EsActivo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Relaciones
        public virtual ICollection<Boleto> Boletos { get; set; } = new List<Boleto>();
    }
}

