using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("AuditoriaCambios")]
    public class AuditoriaCambio
    {
        [Key]
        public long AuditoriaID { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string TablaAfectada { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string RegistroID { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string TipoOperacion { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
        
        [MaxLength(450)]
        public string? UsuarioID { get; set; }
        
        [Column(TypeName = "nvarchar(max)")]
        public string? ValoresAnteriores { get; set; } // JSON
        
        [Column(TypeName = "nvarchar(max)")]
        public string? ValoresNuevos { get; set; } // JSON
        
        public DateTime FechaHoraCambio { get; set; } = DateTime.Now;
        
        // Relaciones
        [ForeignKey(nameof(UsuarioID))]
        public virtual ClApplicationUser? Usuario { get; set; }
    }
}

