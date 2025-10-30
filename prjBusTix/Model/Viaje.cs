using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Viajes")]
    public class Viaje
    {
        [Key]
        public int ViajeID { get; set; }
        
        public int EventoID { get; set; }
        
        public int PlantillaRutaID { get; set; }
        
        public int? UnidadID { get; set; }
        
        [MaxLength(450)]
        public string? ChoferID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string CodigoViaje { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string TipoViaje { get; set; } = string.Empty; // Ida, Regreso
        
        [Required]
        public DateTime FechaSalida { get; set; }
        
        public DateTime? FechaLlegadaEstimada { get; set; }
        
        public int CupoTotal { get; set; }
        
        public int AsientosDisponibles { get; set; }
        
        public int AsientosVendidos { get; set; } = 0;
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioBase { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal CargoServicio { get; set; } = 0;
        
        public bool VentasAbiertas { get; set; } = true;
        
        public int Estatus { get; set; } = 1;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        [MaxLength(450)]
        public string? CreadoPor { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(EventoID))]
        public virtual Evento Evento { get; set; } = null!;
        
        [ForeignKey(nameof(PlantillaRutaID))]
        public virtual PlantillaRuta PlantillaRuta { get; set; } = null!;
        
        [ForeignKey(nameof(UnidadID))]
        public virtual Unidad? Unidad { get; set; }
        
        [ForeignKey(nameof(ChoferID))]
        public virtual ClApplicationUser? Chofer { get; set; }
        
        [ForeignKey(nameof(Estatus))]
        public virtual EstatusGeneral EstatusNavigation { get; set; } = null!;
        
        [ForeignKey(nameof(CreadoPor))]
        public virtual ClApplicationUser? Creador { get; set; }
        
        public virtual ICollection<ViajeStaff> Staff { get; set; } = new List<ViajeStaff>();
        public virtual ICollection<ParadaViaje> Paradas { get; set; } = new List<ParadaViaje>();
        public virtual ICollection<Boleto> Boletos { get; set; } = new List<Boleto>();
        public virtual ICollection<ManifiestoPasajero> ManifiestoPasajeros { get; set; } = new List<ManifiestoPasajero>();
        public virtual ICollection<RegistroValidacion> RegistrosValidacion { get; set; } = new List<RegistroValidacion>();
        public virtual ICollection<Incidencia> Incidencias { get; set; } = new List<Incidencia>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    }
}

