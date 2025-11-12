using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Unidades
{
    public class CreateUnidadDto
    {
        [Required]
        [MaxLength(50)]
        public string NumeroEconomico { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Placas { get; set; }
        
        [MaxLength(100)]
        public string? Marca { get; set; }
        
        [MaxLength(100)]
        public string? Modelo { get; set; }
        
        [Range(1900, 2100, ErrorMessage = "Año fuera de rango válido")] 
        public int? Año { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string TipoUnidad { get; set; }
        
        [Range(1, 200, ErrorMessage = "Capacidad debe ser mayor a 0")]
        public int CapacidadAsientos { get; set; }
        
        public bool TieneClimatizacion { get; set; } = true;
        public bool TieneBaño { get; set; } = false;
        public bool TieneWifi { get; set; } = false;
        
        [Url]
        [MaxLength(512)]
        public string? UrlFoto { get; set; }
        
        public int Estatus { get; set; } = 1;
    }
}
