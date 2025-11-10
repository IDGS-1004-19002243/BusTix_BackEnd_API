﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prjBusTix.Model
{
    [Table("Estatus_General")]
    public class EstatusGeneral
    {
        [Key]
        public int Id_Estatus { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Descripcion { get; set; }
        
        [MaxLength(50)]
        public string? Categoria { get; set; }
        
        public int Orden { get; set; } = 0;
        
        public bool EsActivo { get; set; } = true;
        
        // Relaciones
        public virtual ICollection<ClApplicationUser> Usuarios { get; set; } = new List<ClApplicationUser>();
        public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();
        public virtual ICollection<Unidad> Unidades { get; set; } = new List<Unidad>();
        public virtual ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
        public virtual ICollection<Boleto> Boletos { get; set; } = new List<Boleto>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public virtual ICollection<Incidencia> Incidencias { get; set; } = new List<Incidencia>();
        public virtual ICollection<ManifiestoPasajero> ManifiestoPasajeros { get; set; } = new List<ManifiestoPasajero>();
    }
}

