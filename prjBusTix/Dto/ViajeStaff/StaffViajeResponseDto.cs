namespace prjBusTix.Dto.ViajeStaff
{
    public class StaffViajeResponseDto
    {
        public int AsignacionID { get; set; }
        public int ViajeID { get; set; }
        public string StaffID { get; set; } = string.Empty;
        public string StaffNombre { get; set; } = string.Empty;
        public string? StaffEmail { get; set; }
        public string? StaffTelefono { get; set; }
        public string RolEnViaje { get; set; } = string.Empty;
        public DateTime FechaAsignacion { get; set; }
        public string? Observaciones { get; set; }
    }
}