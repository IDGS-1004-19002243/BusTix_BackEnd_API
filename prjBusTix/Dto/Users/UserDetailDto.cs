namespace prjBusTix.Dto.Users;

public class UserDetailDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? NombreCompleto { get; set; }
    public string[]? Roles { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public int AccessFailedCount { get; set; }
    
    // Campos de Identity
    public int Estatus { get; set; }
    public string? EstatusNombre { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public bool EmailConfirmed { get; set; }
}

