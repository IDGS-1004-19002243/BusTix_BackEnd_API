using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Eventos")]
    public class Evento
    {
        [Key]
        public int EventoID { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string Nombre { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(max)")]
        public string? Descripcion { get; set; }
        
        [MaxLength(50)]
        public string? TipoEvento { get; set; }
        
        [Required]
        [Column(TypeName = "date")]
        public DateTime Fecha { get; set; }
        
        [Column(TypeName = "time")]
        public TimeSpan? HoraInicio { get; set; }
        
        [MaxLength(256)]
        public string? Recinto { get; set; }
        
        [MaxLength(512)]
        public string? Direccion { get; set; }
        
        [MaxLength(100)]
        public string? Ciudad { get; set; }
        
        [MaxLength(100)]
        public string? Estado { get; set; }
        
        [Column(TypeName = "decimal(10,8)")]
        public decimal? UbicacionLat { get; set; }
        
        [Column(TypeName = "decimal(11,8)")]
        public decimal? UbicacionLong { get; set; }
        
        [MaxLength(512)]
        public string? UrlImagen { get; set; }
        
        public int Estatus { get; set; } = 1;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        [MaxLength(450)]
        public string? CreadoPor { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(Estatus))]
        public virtual EstatusGeneral EstatusNavigation { get; set; } = null!;
        
        [ForeignKey(nameof(CreadoPor))]
        public virtual ClApplicationUser? Creador { get; set; }
        
        public virtual ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
    }
}

