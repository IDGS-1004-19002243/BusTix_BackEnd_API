using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using prjBusTix.Data;
using prjBusTix.Dto.Unidades;
using prjBusTix.Model;

namespace prjBusTix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnidadesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UnidadesController> _logger;

        public UnidadesController(AppDbContext context, ILogger<UnidadesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Unidades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnidadResponseDto>>> GetUnidades([FromQuery] bool? activos = null, [FromQuery] string? search = null)
        {
            var query = _context.Unidades.AsQueryable();
            if (activos.HasValue)
                query = query.Where(u => (u.Estatus > 0) == activos.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.NumeroEconomico.Contains(search) || u.Placas.Contains(search) || (u.Marca != null && u.Marca.Contains(search)) || (u.Modelo != null && u.Modelo.Contains(search)));

            var unidades = await query
                .OrderByDescending(u => u.FechaAlta)
                .Select(u => new UnidadResponseDto
                {
                    Id = u.UnidadID,
                    NumeroEconomico = u.NumeroEconomico,
                    Placas = u.Placas,
                    Marca = u.Marca,
                    Modelo = u.Modelo,
                    Año = u.Año,
                    TipoUnidad = u.TipoUnidad,
                    CapacidadAsientos = u.CapacidadAsientos,
                    TieneClimatizacion = u.TieneClimatizacion,
                    TieneBaño = u.TieneBaño,
                    TieneWifi = u.TieneWifi,
                    UrlFoto = u.UrlFoto,
                    Estatus = u.Estatus,
                    FechaAlta = u.FechaAlta
                })
                .ToListAsync();

            return Ok(unidades);
        }

        // GET: api/Unidades/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UnidadResponseDto>> GetUnidad(int id)
        {
            var unidad = await _context.Unidades.FindAsync(id);
            if (unidad == null)
                return NotFound();

            var dto = new UnidadResponseDto
            {
                Id = unidad.UnidadID,
                NumeroEconomico = unidad.NumeroEconomico,
                Placas = unidad.Placas,
                Marca = unidad.Marca,
                Modelo = unidad.Modelo,
                Año = unidad.Año,
                TipoUnidad = unidad.TipoUnidad,
                CapacidadAsientos = unidad.CapacidadAsientos,
                TieneClimatizacion = unidad.TieneClimatizacion,
                TieneBaño = unidad.TieneBaño,
                TieneWifi = unidad.TieneWifi,
                UrlFoto = unidad.UrlFoto,
                Estatus = unidad.Estatus,
                FechaAlta = unidad.FechaAlta
            };

            return Ok(dto);
        }

        // POST: api/Unidades
        [HttpPost]
        public async Task<ActionResult<UnidadResponseDto>> CreateUnidad([FromBody] CreateUnidadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar unicidad de NumeroEconomico y Placas
            var dupNE = await _context.Unidades.AnyAsync(u => u.NumeroEconomico == dto.NumeroEconomico);
            if (dupNE)
                return BadRequest(new { message = "Ya existe una unidad con ese Número Económico" });

            var dupPlacas = await _context.Unidades.AnyAsync(u => u.Placas == dto.Placas);
            if (dupPlacas)
                return BadRequest(new { message = "Ya existe una unidad con esas placas" });

            var unidad = new Unidad
            {
                NumeroEconomico = dto.NumeroEconomico,
                Placas = dto.Placas,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Año = dto.Año,
                TipoUnidad = dto.TipoUnidad,
                CapacidadAsientos = dto.CapacidadAsientos,
                TieneClimatizacion = dto.TieneClimatizacion,
                TieneBaño = dto.TieneBaño,
                TieneWifi = dto.TieneWifi,
                UrlFoto = dto.UrlFoto,
                Estatus = dto.Estatus
            };

            _context.Unidades.Add(unidad);
            await _context.SaveChangesAsync();

            var resultDto = new UnidadResponseDto
            {
                Id = unidad.UnidadID,
                NumeroEconomico = unidad.NumeroEconomico,
                Placas = unidad.Placas,
                Marca = unidad.Marca,
                Modelo = unidad.Modelo,
                Año = unidad.Año,
                TipoUnidad = unidad.TipoUnidad,
                CapacidadAsientos = unidad.CapacidadAsientos,
                TieneClimatizacion = unidad.TieneClimatizacion,
                TieneBaño = unidad.TieneBaño,
                TieneWifi = unidad.TieneWifi,
                UrlFoto = unidad.UrlFoto,
                Estatus = unidad.Estatus,
                FechaAlta = unidad.FechaAlta
            };

            _logger.LogInformation("Unidad creada: {NumeroEconomico} / {Placas}", unidad.NumeroEconomico, unidad.Placas);
            return CreatedAtAction(nameof(GetUnidad), new { id = unidad.UnidadID }, resultDto);
        }

        // PUT: api/Unidades/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUnidad(int id, [FromBody] UpdateUnidadDto dto)
        {
            var unidad = await _context.Unidades.FindAsync(id);
            if (unidad == null)
                return NotFound();

            // Validar unicidad si cambian los campos clave
            if (!string.IsNullOrWhiteSpace(dto.NumeroEconomico) && dto.NumeroEconomico != unidad.NumeroEconomico)
            {
                var dupNE = await _context.Unidades.AnyAsync(u => u.NumeroEconomico == dto.NumeroEconomico && u.UnidadID != id);
                if (dupNE)
                    return BadRequest(new { message = "Ya existe una unidad con ese Número Económico" });
            }
            if (!string.IsNullOrWhiteSpace(dto.Placas) && dto.Placas != unidad.Placas)
            {
                var dupPlacas = await _context.Unidades.AnyAsync(u => u.Placas == dto.Placas && u.UnidadID != id);
                if (dupPlacas)
                    return BadRequest(new { message = "Ya existe una unidad con esas placas" });
            }

            if (dto.NumeroEconomico != null) unidad.NumeroEconomico = dto.NumeroEconomico;
            if (dto.Placas != null) unidad.Placas = dto.Placas;
            if (dto.Marca != null) unidad.Marca = dto.Marca;
            if (dto.Modelo != null) unidad.Modelo = dto.Modelo;
            if (dto.Año.HasValue) unidad.Año = dto.Año;
            if (dto.TipoUnidad != null) unidad.TipoUnidad = dto.TipoUnidad;
            if (dto.CapacidadAsientos.HasValue) unidad.CapacidadAsientos = dto.CapacidadAsientos.Value;
            if (dto.TieneClimatizacion.HasValue) unidad.TieneClimatizacion = dto.TieneClimatizacion.Value;
            if (dto.TieneBaño.HasValue) unidad.TieneBaño = dto.TieneBaño.Value;
            if (dto.TieneWifi.HasValue) unidad.TieneWifi = dto.TieneWifi.Value;
            if (dto.UrlFoto != null) unidad.UrlFoto = dto.UrlFoto;
            if (dto.Estatus.HasValue) unidad.Estatus = dto.Estatus.Value;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Unidad actualizada: {Id}", id);
            return NoContent();
        }

        // DELETE: api/Unidades/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnidad(int id)
        {
            var unidad = await _context.Unidades
                .Include(u => u.Viajes)
                .FirstOrDefaultAsync(u => u.UnidadID == id);
            if (unidad == null)
                return NotFound();

            // Si tiene viajes asociados activos, no eliminar físicamente
            if (unidad.Viajes.Any())
            {
                unidad.Estatus = 0; // Inactivo (soft delete)
                await _context.SaveChangesAsync();
                _logger.LogInformation("Unidad {Id} desactivada (soft delete) por tener viajes asociados", id);
                return Ok(new { message = "Unidad desactivada por tener viajes asociados" });
            }

            _context.Unidades.Remove(unidad);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Unidad {Id} eliminada", id);
            return NoContent();
        }
    }
}