using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Unidades
{
    public class UpdateUnidadDto
    {
        [MaxLength(50)]
        public string? NumeroEconomico { get; set; }
        [MaxLength(50)]
        public string? Placas { get; set; }
        [MaxLength(100)]
        public string? Marca { get; set; }
        [MaxLength(100)]
        public string? Modelo { get; set; }
        [Range(1900, 2100)]
        public int? Año { get; set; }
        [MaxLength(50)]
        public string? TipoUnidad { get; set; }
        [Range(1, 200)]
        public int? CapacidadAsientos { get; set; }
        public bool? TieneClimatizacion { get; set; }
        public bool? TieneBaño { get; set; }
        public bool? TieneWifi { get; set; }
        [Url]
        [MaxLength(512)]
        public string? UrlFoto { get; set; }
        public int? Estatus { get; set; }
    }
}
