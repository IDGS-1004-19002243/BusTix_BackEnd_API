using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Boletos")]
    public class Boleto
    {
        [Key]
        public int BoletoID { get; set; }
        
        public int ViajeID { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string ClienteID { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string CodigoBoleto { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string CodigoQR { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string? NumeroAsiento { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string NombrePasajero { get; set; } = string.Empty;
        
        [MaxLength(256)]
        public string? EmailPasajero { get; set; }
        
        [MaxLength(50)]
        public string? TelefonoPasajero { get; set; }
        
        public int? ParadaAbordajeID { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioBase { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Descuento { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal CargoServicio { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal IVA { get; set; } = 0;
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioTotal { get; set; }
        
        public int? CuponAplicadoID { get; set; }
        
        public DateTime FechaCompra { get; set; } = DateTime.Now;
        
        public int Estatus { get; set; } = 1;
        
        public DateTime? FechaValidacion { get; set; }
        
        [MaxLength(450)]
        public string? ValidadoPor { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje Viaje { get; set; } = null!;
        
        [ForeignKey(nameof(ClienteID))]
        public virtual ClApplicationUser Cliente { get; set; } = null!;
        
        [ForeignKey(nameof(ParadaAbordajeID))]
        public virtual ParadaViaje? ParadaAbordaje { get; set; }
        
        [ForeignKey(nameof(Estatus))]
        public virtual EstatusGeneral EstatusNavigation { get; set; } = null!;
        
        [ForeignKey(nameof(ValidadoPor))]
        public virtual ClApplicationUser? Validador { get; set; }
        
        [ForeignKey(nameof(CuponAplicadoID))]
        public virtual Cupon? CuponAplicado { get; set; }
        
        public virtual ManifiestoPasajero? ManifiestoPasajero { get; set; }
        public virtual ICollection<PagoBoleto> PagosBoletos { get; set; } = new List<PagoBoleto>();
        public virtual ICollection<RegistroValidacion> RegistrosValidacion { get; set; } = new List<RegistroValidacion>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    }
}

