namespace prjBusTix.Dto.ViajeStaff
{
    public class AsignarStaffViajeDto
    {
        public string StaffID { get; set; } = string.Empty;
        public string RolEnViaje { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }
}