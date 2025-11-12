using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Cupones;
using prjBusTix.Model;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuponesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CuponesController> _logger;

    public CuponesController(AppDbContext context, ILogger<CuponesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ADMIN: listar cupones
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<CuponResponseDto>>> GetCupones(
        [FromQuery] bool? activos,
        [FromQuery] string? search)
    {
        var query = _context.Cupones.AsQueryable();
        if (activos.HasValue)
            query = query.Where(c => c.EsActivo == activos.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Codigo.Contains(search) || (c.Descripcion != null && c.Descripcion.Contains(search)));

        var list = await query
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new CuponResponseDto
            {
                CuponID = c.CuponID,
                Codigo = c.Codigo,
                Descripcion = c.Descripcion,
                TipoDescuento = c.TipoDescuento ?? string.Empty,
                ValorDescuento = c.ValorDescuento,
                UsosMaximos = c.UsosMaximos,
                UsosRealizados = c.UsosRealizados,
                FechaInicio = c.FechaInicio,
                FechaExpiracion = c.FechaExpiracion,
                EsActivo = c.EsActivo,
                FechaCreacion = c.FechaCreacion,
                EstaVigente = EstaVigente(c)
            })
            .ToListAsync();

        return Ok(list);
    }

    // ADMIN: obtener cupon por id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CuponResponseDto>> GetCupon(int id)
    {
        var c = await _context.Cupones.FindAsync(id);
        if (c == null) return NotFound(new { message = "Cupón no encontrado" });
        return Ok(Map(c));
    }

    // ADMIN: crear
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CuponResponseDto>> Crear([FromBody] CreateCuponDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Datos inválidos", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }
        if (string.IsNullOrWhiteSpace(dto.Codigo))
            return BadRequest(new { message = "Código es requerido" });

        var existe = await _context.Cupones.AnyAsync(x => x.Codigo == dto.Codigo.Trim().ToUpper());
        if (existe) return BadRequest(new { message = "Ya existe un cupón con ese código" });

        var cupon = new Cupon
        {
            Codigo = dto.Codigo.Trim().ToUpper(),
            Descripcion = dto.Descripcion,
            TipoDescuento = dto.TipoDescuento,
            ValorDescuento = dto.ValorDescuento,
            UsosMaximos = dto.UsosMaximos,
            FechaInicio = dto.FechaInicio,
            FechaExpiracion = dto.FechaExpiracion,
            EsActivo = dto.EsActivo,
            FechaCreacion = DateTime.Now
        };
        _context.Cupones.Add(cupon);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cupón creado: {Codigo} (ID {Id})", cupon.Codigo, cupon.CuponID);
        return CreatedAtAction(nameof(GetCupon), new { id = cupon.CuponID }, Map(cupon));
    }

    // ADMIN: actualizar
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CuponResponseDto>> Actualizar(int id, [FromBody] UpdateCuponDto dto)
    {
        var cupon = await _context.Cupones.FindAsync(id);
        if (cupon == null) return NotFound(new { message = "Cupón no encontrado" });

        if (dto.Descripcion != null) cupon.Descripcion = dto.Descripcion;
        if (!string.IsNullOrWhiteSpace(dto.TipoDescuento)) cupon.TipoDescuento = dto.TipoDescuento;
        if (dto.ValorDescuento.HasValue) cupon.ValorDescuento = dto.ValorDescuento.Value;
        if (dto.UsosMaximos.HasValue) cupon.UsosMaximos = dto.UsosMaximos.Value;
        if (dto.FechaInicio.HasValue) cupon.FechaInicio = dto.FechaInicio;
        if (dto.FechaExpiracion.HasValue) cupon.FechaExpiracion = dto.FechaExpiracion;
        if (dto.EsActivo.HasValue) cupon.EsActivo = dto.EsActivo.Value;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Cupón actualizado: {Codigo} (ID {Id})", cupon.Codigo, cupon.CuponID);
        return Ok(Map(cupon));
    }

    // ADMIN: desactivar (soft-delete)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Desactivar(int id)
    {
        var cupon = await _context.Cupones.FindAsync(id);
        if (cupon == null) return NotFound(new { message = "Cupón no encontrado" });
        cupon.EsActivo = false;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cupón desactivado: {Codigo} (ID {Id})", cupon.Codigo, cupon.CuponID);
        return Ok(new { message = "Cupón desactivado" });
    }

    // PÚBLICO: validar por código
    [HttpGet("validar")]
    [AllowAnonymous]
    public async Task<ActionResult<ValidarCuponResponseDto>> Validar([FromQuery] string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return BadRequest(new { message = "Código requerido" });

        var c = await _context.Cupones.FirstOrDefaultAsync(x => x.Codigo == codigo.Trim().ToUpper());
        if (c == null)
        {
            _logger.LogInformation("Cupón no encontrado en validación: {Codigo}", codigo);
            return Ok(new ValidarCuponResponseDto { Valido = false, Message = "Cupón no encontrado" });
        }

        var (valido, error) = ValidarCuponEntidad(c);
        if (!valido)
        {
            _logger.LogInformation("Cupón inválido {Codigo}: {Error}", codigo, error);
            return Ok(new ValidarCuponResponseDto
            {
                Valido = false,
                Message = error!
            });
        }

        _logger.LogInformation("Cupón válido {Codigo}", codigo);
        return Ok(new ValidarCuponResponseDto
        {
            Valido = true,
            Message = "Cupón válido",
            CuponID = c.CuponID,
            Codigo = c.Codigo,
            TipoDescuento = c.TipoDescuento,
            ValorDescuento = c.ValorDescuento,
            FechaExpiracion = c.FechaExpiracion,
            UsosMaximos = c.UsosMaximos,
            UsosRealizados = c.UsosRealizados
        });
    }

    private static bool EstaVigente(Cupon c)
    {
        var ahora = DateTime.Now;
        if (!c.EsActivo) return false;
        if (c.FechaInicio.HasValue && ahora < c.FechaInicio.Value) return false;
        if (c.FechaExpiracion.HasValue && ahora > c.FechaExpiracion.Value) return false;
        if (c.UsosMaximos.HasValue && c.UsosRealizados >= c.UsosMaximos.Value) return false;
        return true;
    }

    private static (bool valido, string? error) ValidarCuponEntidad(Cupon c)
    {
        var ahora = DateTime.Now;
        if (!c.EsActivo) return (false, "Cupón inactivo");
        if (c.FechaInicio.HasValue && ahora < c.FechaInicio.Value) return (false, "Aún no vigente");
        if (c.FechaExpiracion.HasValue && ahora > c.FechaExpiracion.Value) return (false, "Cupón expirado");
        if (c.UsosMaximos.HasValue && c.UsosRealizados >= c.UsosMaximos.Value) return (false, "Límite de usos alcanzado");
        if (string.IsNullOrWhiteSpace(c.TipoDescuento)) return (false, "Tipo de descuento no configurado");
        if (c.ValorDescuento <= 0) return (false, "Valor de descuento inválido");
        return (true, null);
    }

    private static CuponResponseDto Map(Cupon c)
    {
        return new CuponResponseDto
        {
            CuponID = c.CuponID,
            Codigo = c.Codigo,
            Descripcion = c.Descripcion,
            TipoDescuento = c.TipoDescuento ?? string.Empty,
            ValorDescuento = c.ValorDescuento,
            UsosMaximos = c.UsosMaximos,
            UsosRealizados = c.UsosRealizados,
            FechaInicio = c.FechaInicio,
            FechaExpiracion = c.FechaExpiracion,
            EsActivo = c.EsActivo,
            FechaCreacion = c.FechaCreacion,
            EstaVigente = EstaVigente(c)
        };
    }
}
