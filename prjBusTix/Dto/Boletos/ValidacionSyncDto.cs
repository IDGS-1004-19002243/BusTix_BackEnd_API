using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Boletos;

/// <summary>
/// DTO para sincronización batch de validaciones offline
/// </summary>
public class ValidacionSyncDto
{
    [Required]
    public int BoletoID { get; set; }

    [Required]
    public int ViajeID { get; set; }

    [Required]
    public DateTime FechaHoraValidacion { get; set; }

    [Required]
    [MaxLength(50)]
    public string Resultado { get; set; } = string.Empty; // "Aprobado", "Rechazado"

    [MaxLength(50)]
    public string TipoValidacion { get; set; } = "EscaneoQR";

    [Range(-90, 90)]
    public decimal? EstacionLat { get; set; }

    [Range(-180, 180)]
    public decimal? EstacionLong { get; set; }

    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID único generado por el dispositivo para idempotencia
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DeviceValidationId { get; set; } = string.Empty;

    /// <summary>
    /// ID del dispositivo que realizó la validación
    /// </summary>
    [MaxLength(100)]
    public string? DeviceId { get; set; }
}

