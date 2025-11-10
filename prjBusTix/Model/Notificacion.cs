using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Notificaciones")]
    public class Notificacion
    {
        [Key]
        public int NotificacionID { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UsuarioID { get; set; } = string.Empty;
        
        public int? ViajeID { get; set; }
        
        public int? BoletoID { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Mensaje { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? TipoNotificacion { get; set; } // Recordatorio, Cambio
        
        public bool EnviarPush { get; set; } = true;
        
        public bool EnviarEmail { get; set; } = false;
        
        public bool FueEnviada { get; set; } = false;
        
        public DateTime? FechaEnvio { get; set; }
        
        public bool FueLeida { get; set; } = false;
        
        public DateTime? FechaLectura { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Relaciones
        [ForeignKey(nameof(UsuarioID))]
        public virtual ClApplicationUser Usuario { get; set; } = null!;
        
        [ForeignKey(nameof(ViajeID))]
        public virtual Viaje? Viaje { get; set; }
        
        [ForeignKey(nameof(BoletoID))]
        public virtual Boleto? Boleto { get; set; }
    }
}

