using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    public class ClApplicationUser : IdentityUser
    {
        // Campos BusTix según esquema SQL
        [Required]
        [MaxLength(256)]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? TipoDocumento { get; set; }
        
        [MaxLength(100)]
        public string? NumeroDocumento { get; set; }
        
        public DateTime? FechaNacimiento { get; set; }
        
        [MaxLength(512)]
        public string? Direccion { get; set; }
        
        [MaxLength(100)]
        public string? Ciudad { get; set; }
        
        [MaxLength(100)]
        public string? Estado { get; set; }
        
        [MaxLength(10)]
        public string? CodigoPostal { get; set; }
        
        public bool NotificacionesPush { get; set; } = true;
        public bool NotificacionesEmail { get; set; } = true;
        
        [MaxLength(512)]
        public string? UrlFotoPerfil { get; set; }
        
        public int Estatus { get; set; } = 1;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        public DateTime? UltimaConexion { get; set; }
        
        [MaxLength(50)]
        public string? NumeroEmpleado { get; set; }
        
        // Campos adicionales para funcionalidad JWT y sesiones
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        
        // Relaciones
        [ForeignKey(nameof(Estatus))]
        public virtual EstatusGeneral? EstatusNavigation { get; set; }
        
        public virtual ICollection<DispositivoUsuario> Dispositivos { get; set; } = new List<DispositivoUsuario>();
        public virtual ICollection<Evento> EventosCreados { get; set; } = new List<Evento>();
        public virtual ICollection<PlantillaRuta> PlantillasRutasCreadas { get; set; } = new List<PlantillaRuta>();
        public virtual ICollection<Viaje> ViajesCreados { get; set; } = new List<Viaje>();
        public virtual ICollection<Viaje> ViajesComoChofer { get; set; } = new List<Viaje>();
        public virtual ICollection<ViajeStaff> AsignacionesStaff { get; set; } = new List<ViajeStaff>();
        public virtual ICollection<Boleto> Boletos { get; set; } = new List<Boleto>();
        public virtual ICollection<Boleto> BoletosValidados { get; set; } = new List<Boleto>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public virtual ICollection<Incidencia> IncidenciasReportadas { get; set; } = new List<Incidencia>();
        public virtual ICollection<Incidencia> IncidenciasAsignadas { get; set; } = new List<Incidencia>();
        public virtual ICollection<RegistroValidacion> Validaciones { get; set; } = new List<RegistroValidacion>();
        public virtual ICollection<ManifiestoPasajero> ManifiestosValidados { get; set; } = new List<ManifiestoPasajero>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
        public virtual ICollection<AuditoriaCambio> CambiosAuditados { get; set; } = new List<AuditoriaCambio>();
    }
}

