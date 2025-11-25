using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.ViajeStaff;
using prjBusTix.Model;
using System.Threading.Tasks;

namespace prjBusTix.Controllers
{
    [ApiController]
    [Route("api/viajes/{viajeId}/staff")]
    [Authorize]
    public class ViajeStaffController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ViajeStaffController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/viajes/{viajeId}/staff
        [HttpPost]
        public async Task<ActionResult<StaffViajeResponseDto>> AsignarStaff(int viajeId, [FromBody] AsignarStaffViajeDto dto)
        {
            // 1. Validar que el viaje existe
            var viaje = await _context.Viajes.FindAsync(viajeId);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            // 2. Validar que el usuario existe
            var staff = await _context.Users.FindAsync(dto.StaffID);
            if (staff == null)
                return BadRequest(new { message = "El usuario especificado no existe" });

            //3. Validar que el usuario está activo
            if (staff.Estatus != 1)
                return BadRequest(new { message = "El usuario no está activo en el sistema" });

            // 4. Validar que no esté ya asignado a este viaje
            var yaAsignado = await _context.ViajesStaff
                .AnyAsync(vs => vs.ViajeID == viajeId && vs.StaffID == dto.StaffID);
            
            if (yaAsignado)
                return BadRequest(new { message = "Este usuario ya está asignado a este viaje" });

            // 5. Crear la asignación
            var asignacion = new ViajeStaff
            {
                ViajeID = viajeId,
                StaffID = dto.StaffID,
                RolEnViaje = dto.RolEnViaje,
                FechaAsignacion = DateTime.Now,
                Observaciones = dto.Observaciones
            };

            _context.ViajesStaff.Add(asignacion);
            await _context.SaveChangesAsync();

            var response = new StaffViajeResponseDto
            {
                AsignacionID = asignacion.AsignacionID,
                ViajeID = asignacion.ViajeID,
                StaffID = asignacion.StaffID,
                StaffNombre = staff.NombreCompleto,
                StaffEmail = staff.Email,
                StaffTelefono = staff.PhoneNumber,
                RolEnViaje = asignacion.RolEnViaje,
                FechaAsignacion = asignacion.FechaAsignacion,
                Observaciones = asignacion.Observaciones
            };

            return CreatedAtAction(nameof(GetStaffDeViaje), new { viajeId }, response);
        }

        // GET: api/viajes/{viajeId}/staff
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaffViajeResponseDto>>> GetStaffDeViaje(int viajeId)
        {
            var viaje = await _context.Viajes.FindAsync(viajeId);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            var staff = await _context.ViajesStaff
                .Include(vs => vs.Staff)
                .Where(vs => vs.ViajeID == viajeId)
                .OrderBy(vs => vs.FechaAsignacion)
                .Select(vs => new StaffViajeResponseDto
                {
                    AsignacionID = vs.AsignacionID,
                    ViajeID = vs.ViajeID,
                    StaffID = vs.StaffID,
                    StaffNombre = vs.Staff.NombreCompleto,
                    StaffEmail = vs.Staff.Email,
                    StaffTelefono = vs.Staff.PhoneNumber,
                    RolEnViaje = vs.RolEnViaje,
                    FechaAsignacion = vs.FechaAsignacion,
                    Observaciones = vs.Observaciones
                })
                .ToListAsync();

            return Ok(staff);
        }

        // PUT: api/viajes/{viajeId}/staff/{asignacionId}
        [HttpPut("{asignacionId}")]
        public async Task<ActionResult<StaffViajeResponseDto>> UpdateStaffViaje(int viajeId, int asignacionId, [FromBody] UpdateStaffViajeDto dto)
        {
            var asignacion = await _context.ViajesStaff
                .Include(vs => vs.Staff)
                .FirstOrDefaultAsync(vs => vs.AsignacionID == asignacionId && vs.ViajeID == viajeId);

            if (asignacion == null)
                return NotFound(new { message = "Asignación no encontrada" });

            if (dto.RolEnViaje != null)
                asignacion.RolEnViaje = dto.RolEnViaje;
            if (dto.Observaciones != null)
                asignacion.Observaciones = dto.Observaciones;

            await _context.SaveChangesAsync();

            var response = new StaffViajeResponseDto
            {
                AsignacionID = asignacion.AsignacionID,
                ViajeID = asignacion.ViajeID,
                StaffID = asignacion.StaffID,
                StaffNombre = asignacion.Staff.NombreCompleto,
                StaffEmail = asignacion.Staff.Email,
                StaffTelefono = asignacion.Staff.PhoneNumber,
                RolEnViaje = asignacion.RolEnViaje,
                FechaAsignacion = asignacion.FechaAsignacion,
                Observaciones = asignacion.Observaciones
            };

            return Ok(response);
        }

        // DELETE: api/viajes/{viajeId}/staff/{asignacionId}
        [HttpDelete("{asignacionId}")]
        public async Task<ActionResult> DesasignarStaff(int viajeId, int asignacionId)
        {
            var asignacion = await _context.ViajesStaff
                .FirstOrDefaultAsync(vs => vs.AsignacionID == asignacionId && vs.ViajeID == viajeId);

            if (asignacion == null)
                return NotFound(new { message = "Asignación no encontrada" });

            _context.ViajesStaff.Remove(asignacion);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff desasignado correctamente" });
        }
    }
}