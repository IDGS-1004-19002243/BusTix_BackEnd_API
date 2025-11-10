﻿using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;
    
    [Required]
    public string NombreCompleto { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public List<string>? Roles { get; set; }
    
    // Nuevos campos de identificación
    public string? TipoDocumento { get; set; }  // Ej: "INE", "Pasaporte", "RFC", "Licencia"
    public string? NumeroDocumento { get; set; }
}