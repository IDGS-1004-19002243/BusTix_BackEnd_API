using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Dto.Unidades;
using prjBusTix.Model;
using prjBusTix.Data;

namespace prjBusTix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnidadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnidadesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Unidades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnidadResponseDto>>> GetUnidades()
        {
            var unidades = await _context.Unidades
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

            return CreatedAtAction(nameof(GetUnidad), new { id = unidad.UnidadID }, resultDto);
        }

        // PUT: api/Unidades/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUnidad(int id, [FromBody] UpdateUnidadDto dto)
        {
            var unidad = await _context.Unidades.FindAsync(id);
            if (unidad == null)
                return NotFound();

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
            return NoContent();
        }

        // DELETE: api/Unidades/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnidad(int id)
        {
            var unidad = await _context.Unidades.FindAsync(id);
            if (unidad == null)
                return NotFound();

            _context.Unidades.Remove(unidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}