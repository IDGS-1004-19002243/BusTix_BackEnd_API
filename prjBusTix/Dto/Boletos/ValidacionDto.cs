using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Boletos;

/// <summary>
/// DTO para validar un boleto (escaneo QR)
/// </summary>
public class ValidacionDto
{
    [Required(ErrorMessage = "El ID del viaje es requerido")]
    public int ViajeID { get; set; }

    [Required(ErrorMessage = "El código QR es requerido")]
    [MaxLength(512)]
    public string CodigoQR { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de validación: "EscaneoQR", "Manual"
    /// </summary>
    [MaxLength(50)]
    public string? TipoValidacion { get; set; } = "EscaneoQR";

    /// <summary>
    /// Latitud de la estación donde se validó
    /// </summary>
    [Range(-90, 90)]
    public decimal? EstacionLat { get; set; }

    /// <summary>
    /// Longitud de la estación donde se validó
    /// </summary>
    [Range(-180, 180)]
    public decimal? EstacionLong { get; set; }

    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID único generado por el dispositivo para idempotencia
    /// </summary>
    [MaxLength(50)]
    public string? DeviceValidationId { get; set; }
}

