﻿using System.ComponentModel.DataAnnotations;

namespace prjBusTix.Dto.Users;

public class UpdateUserDto
{
    public string? NombreCompleto { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
}

