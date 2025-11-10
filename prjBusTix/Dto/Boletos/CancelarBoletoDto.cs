using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Boletos;

public class CancelarBoletoDto
{
    [MaxLength(500)]
    public string? Motivo { get; set; }
}

public class CambiarAsientoDto
{
    [Required(ErrorMessage = "El nuevo asiento es requerido")]
    [MaxLength(10)]
    public string NuevoAsiento { get; set; } = string.Empty;
}

