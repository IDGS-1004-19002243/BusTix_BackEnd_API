using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("DispositivosUsuario")]
    public class DispositivoUsuario
    {
        [Key]
        public int DispositivoID { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UsuarioID { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string TokenPush { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? TipoDispositivo { get; set; } // iOS, Android
        
        [MaxLength(50)]
        public string? Plataforma { get; set; } // FCM, APNS
        
        public bool EsActivo { get; set; } = true;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        // Relaciones
        [ForeignKey(nameof(UsuarioID))]
        public virtual ClApplicationUser Usuario { get; set; } = null!;
    }
}

