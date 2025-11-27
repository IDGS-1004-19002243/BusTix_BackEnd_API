using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Viajes;
using prjBusTix.Model;
using prjBusTix.Security;
using System.Security.Claims;

namespace prjBusTix.Controllers;

/// <summary>
/// Controlador para la gestión de precios dinámicos por parada de viaje
/// Permite configurar tarifas diferentes según el punto de abordaje
/// </summary>
[ApiController]
[Route("api/viajes/{viajeId}/precios")]
[Authorize]
public class PreciosParadaController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PreciosParadaController> _logger;

    public PreciosParadaController(AppDbContext context, ILogger<PreciosParadaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los precios configurados para un viaje
    /// GET: api/viajes/{viajeId}/precios
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PrecioParadaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PrecioParadaResponseDto>>> GetPreciosViaje(int viajeId)
    {
        try
        {
            var viaje = await _context.Viajes.FindAsync(viajeId);
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });

            var precios = await _context.PreciosParada
                .Include(p => p.ParadaViaje)
                .Include(p => p.Creador)
                .Where(p => p.ViajeID == viajeId && p.EsActivo)
                .Select(p => new PrecioParadaResponseDto
                {
                    PrecioParadaID = p.PrecioParadaID,
                    ViajeID = p.ViajeID,
                    ParadaViajeID = p.ParadaViajeID,
                    NombreParada = p.ParadaViaje != null ? p.ParadaViaje.NombreParada : "Desconocida",
                    PrecioBase = p.PrecioBase,
                    CargoServicio = p.CargoServicio,
                    PrecioTotal = p.PrecioTotal,
                    EsActivo = p.EsActivo,
                    FechaCreacion = p.FechaCreacion,
                    CreadoPor = p.CreadoPor,
                    NombreCreador = p.Creador != null ? p.Creador.NombreCompleto : null,
                    Observaciones = p.Observaciones,
                    OrdenParada = p.ParadaViaje != null ? p.ParadaViaje.OrdenParada : 999
                })
                .ToListAsync();

            // Ordenar en memoria por OrdenParada
            precios = precios.OrderBy(p => p.OrdenParada).ToList();

            return Ok(precios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener precios del viaje {ViajeId}", viajeId);
            return StatusCode(500, new { message = "Error al obtener los precios" });
        }
    }

    /// <summary>
    /// Obtener el precio de una parada específica
    /// GET: api/viajes/{viajeId}/precios/parada/{paradaId}
    /// </summary>
    [HttpGet("parada/{paradaId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PrecioParadaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrecioParadaResponseDto>> GetPrecioParada(int viajeId, int paradaId)
    {
        try
        {
            var precio = await _context.PreciosParada
                .Include(p => p.ParadaViaje)
                .Include(p => p.Creador)
                .Where(p => p.ViajeID == viajeId && p.ParadaViajeID == paradaId && p.EsActivo)
                .Select(p => new PrecioParadaResponseDto
                {
                    PrecioParadaID = p.PrecioParadaID,
                    ViajeID = p.ViajeID,
                    ParadaViajeID = p.ParadaViajeID,
                    NombreParada = p.ParadaViaje != null ? p.ParadaViaje.NombreParada : "Desconocida",
                    PrecioBase = p.PrecioBase,
                    CargoServicio = p.CargoServicio,
                    PrecioTotal = p.PrecioTotal,
                    EsActivo = p.EsActivo,
                    FechaCreacion = p.FechaCreacion,
                    CreadoPor = p.CreadoPor,
                    NombreCreador = p.Creador != null ? p.Creador.NombreCompleto : null,
                    Observaciones = p.Observaciones,
                    OrdenParada = p.ParadaViaje != null ? p.ParadaViaje.OrdenParada : 999
                })
                .FirstOrDefaultAsync();

            if (precio == null)
            {
                // Si no hay precio específico, devolver el precio base del viaje
                var viaje = await _context.Viajes.FindAsync(viajeId);
                var parada = await _context.ParadasViaje.FindAsync(paradaId);
                
                if (viaje == null || parada == null)
                    return NotFound(new { message = "Viaje o parada no encontrada" });

                return Ok(new PrecioParadaResponseDto
                {
                    ViajeID = viajeId,
                    ParadaViajeID = paradaId,
                    NombreParada = parada.NombreParada,
                    PrecioBase = viaje.PrecioBase,
                    CargoServicio = viaje.CargoServicio,
                    PrecioTotal = viaje.PrecioBase + viaje.CargoServicio,
                    EsActivo = true,
                    Observaciones = "Precio base del viaje (sin configuración específica)",
                    OrdenParada = parada.OrdenParada
                });
            }

            return Ok(precio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener precio de parada {ParadaId} del viaje {ViajeId}", paradaId, viajeId);
            return StatusCode(500, new { message = "Error al obtener el precio" });
        }
    }

    /// <summary>
    /// Configurar precios para múltiples paradas de un viaje
    /// POST: api/viajes/{viajeId}/precios/configurar
    /// </summary>
    [HttpPost("configurar")]
    [ClRequirePermission(AppPermissions.Viajes.Create)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ConfigurarPrecios(int viajeId, [FromBody] List<ConfigurarPrecioParadaDto> preciosDto)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Usuario no autenticado" });

                // Validar que el viaje existe
                var viaje = await _context.Viajes
                    .Include(v => v.Paradas)
                    .FirstOrDefaultAsync(v => v.ViajeID == viajeId);
                
                if (viaje == null)
                    return NotFound(new { message = "Viaje no encontrado" });

                // Validar que las paradas existen
                var paradaIds = preciosDto.Select(p => p.ParadaViajeID).ToList();
                var paradasValidas = await _context.ParadasViaje
                    .Where(p => p.ViajeID == viajeId && paradaIds.Contains(p.ParadaViajeID))
                    .Select(p => p.ParadaViajeID)
                    .ToListAsync();

                if (paradasValidas.Count != paradaIds.Count)
                    return BadRequest(new { message = "Una o más paradas no pertenecen al viaje especificado" });

                // Obtener todos los precios existentes para este viaje en una sola consulta
                var preciosExistentes = await _context.PreciosParada
                    .Where(p => p.ViajeID == viajeId)
                    .ToDictionaryAsync(p => p.ParadaViajeID, p => p);

                var preciosCreados = new List<PrecioParada>();
                var preciosActualizados = new List<PrecioParada>();

                foreach (var dto in preciosDto)
                {
                    // Validar precios
                    if (dto.PrecioBase < 0 || dto.CargoServicio < 0)
                    {
                        return BadRequest(new { message = $"Los precios no pueden ser negativos (Parada: {dto.ParadaViajeID})" });
                    }

                    // Buscar si ya existe un precio para esta parada en memoria
                    if (preciosExistentes.TryGetValue(dto.ParadaViajeID, out var precioExistente))
                    {
                        // Actualizar precio existente
                        precioExistente.PrecioBase = dto.PrecioBase;
                        precioExistente.CargoServicio = dto.CargoServicio;
                        precioExistente.PrecioTotal = dto.PrecioBase + dto.CargoServicio;
                        precioExistente.Observaciones = dto.Observaciones;
                        precioExistente.EsActivo = true;
                        
                        preciosActualizados.Add(precioExistente);
                    }
                    else
                    {
                        // Crear nuevo precio
                        var nuevoPrecio = new PrecioParada
                        {
                            ViajeID = viajeId,
                            ParadaViajeID = dto.ParadaViajeID,
                            PrecioBase = dto.PrecioBase,
                            CargoServicio = dto.CargoServicio,
                            PrecioTotal = dto.PrecioBase + dto.CargoServicio,
                            EsActivo = true,
                            FechaCreacion = DateTime.Now,
                            CreadoPor = userId,
                            Observaciones = dto.Observaciones
                        };
                        
                        _context.PreciosParada.Add(nuevoPrecio);
                        preciosCreados.Add(nuevoPrecio);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Precios configurados para viaje {ViajeId}: {Creados} creados, {Actualizados} actualizados por usuario {UserId}",
                    viajeId, preciosCreados.Count, preciosActualizados.Count, userId);

                return Ok(new
                {
                    success = true,
                    message = "Precios configurados exitosamente",
                    preciosCreados = preciosCreados.Count,
                    preciosActualizados = preciosActualizados.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al configurar precios del viaje {ViajeId}", viajeId);
                return StatusCode(500, new { message = "Error al configurar los precios", error = ex.Message });
            }
        });
    }

    /// <summary>
    /// Actualizar el precio de una parada específica
    /// PUT: api/viajes/{viajeId}/precios/{precioId}
    /// </summary>
    [HttpPut("{precioId}")]
    [ClRequirePermission(AppPermissions.Viajes.Edit)]
    [ProducesResponseType(typeof(PrecioParadaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PrecioParadaResponseDto>> ActualizarPrecio(
        int viajeId, 
        int precioId, 
        [FromBody] ConfigurarPrecioParadaDto dto)
    {
        try
        {
            var precio = await _context.PreciosParada
                .Include(p => p.ParadaViaje)
                .FirstOrDefaultAsync(p => p.PrecioParadaID == precioId && p.ViajeID == viajeId);

            if (precio == null)
                return NotFound(new { message = "Precio no encontrado" });

            if (dto.PrecioBase < 0 || dto.CargoServicio < 0)
                return BadRequest(new { message = "Los precios no pueden ser negativos" });

            // Validar consistencia si se envía ParadaViajeID
            if (dto.ParadaViajeID != 0 && dto.ParadaViajeID != precio.ParadaViajeID)
                return BadRequest(new { message = "No se puede cambiar la parada asociada a un precio existente" });

            precio.PrecioBase = dto.PrecioBase;
            precio.CargoServicio = dto.CargoServicio;
            precio.PrecioTotal = dto.PrecioBase + dto.CargoServicio;
            precio.Observaciones = dto.Observaciones;

            await _context.SaveChangesAsync();

            var response = new PrecioParadaResponseDto
            {
                PrecioParadaID = precio.PrecioParadaID,
                ViajeID = precio.ViajeID,
                ParadaViajeID = precio.ParadaViajeID,
                NombreParada = precio.ParadaViaje != null ? precio.ParadaViaje.NombreParada : "Desconocida",
                PrecioBase = precio.PrecioBase,
                CargoServicio = precio.CargoServicio,
                PrecioTotal = precio.PrecioTotal,
                EsActivo = precio.EsActivo,
                FechaCreacion = precio.FechaCreacion,
                CreadoPor = precio.CreadoPor,
                Observaciones = precio.Observaciones
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar precio {PrecioId}", precioId);
            return StatusCode(500, new { message = "Error al actualizar el precio" });
        }
    }

    /// <summary>
    /// Desactivar un precio (soft delete)
    /// DELETE: api/viajes/{viajeId}/precios/{precioId}
    /// </summary>
    [HttpDelete("{precioId}")]
    [ClRequirePermission(AppPermissions.Viajes.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DesactivarPrecio(int viajeId, int precioId)
    {
        try
        {
            var precio = await _context.PreciosParada
                .FirstOrDefaultAsync(p => p.PrecioParadaID == precioId && p.ViajeID == viajeId);

            if (precio == null)
                return NotFound(new { message = "Precio no encontrado" });

            precio.EsActivo = false;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Precio desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar precio {PrecioId}", precioId);
            return StatusCode(500, new { message = "Error al desactivar el precio" });
        }
    }

    /// <summary>
    /// Copiar precios base del viaje a todas las paradas
    /// POST: api/viajes/{viajeId}/precios/copiar-base
    /// </summary>
    [HttpPost("copiar-base")]
    [ClRequirePermission(AppPermissions.Viajes.Create)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CopiarPreciosBase(int viajeId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var viaje = await _context.Viajes
                    .Include(v => v.Paradas)
                    .FirstOrDefaultAsync(v => v.ViajeID == viajeId);

                if (viaje == null)
                    return NotFound(new { message = "Viaje no encontrado" });

                // Obtener IDs de paradas que ya tienen precio configurado
                var paradasConPrecio = await _context.PreciosParada
                    .Where(p => p.ViajeID == viajeId)
                    .Select(p => p.ParadaViajeID)
                    .ToListAsync();
                
                var paradasConPrecioSet = new HashSet<int>(paradasConPrecio);

                var preciosCreados = 0;

                foreach (var parada in viaje.Paradas)
                {
                    // Verificar en memoria si ya tiene precio
                    if (!paradasConPrecioSet.Contains(parada.ParadaViajeID))
                    {
                        var nuevoPrecio = new PrecioParada
                        {
                            ViajeID = viajeId,
                            ParadaViajeID = parada.ParadaViajeID,
                            PrecioBase = viaje.PrecioBase,
                            CargoServicio = viaje.CargoServicio,
                            PrecioTotal = viaje.PrecioBase + viaje.CargoServicio,
                            EsActivo = true,
                            FechaCreacion = DateTime.Now,
                            CreadoPor = userId,
                            Observaciones = "Copiado del precio base del viaje"
                        };

                        _context.PreciosParada.Add(nuevoPrecio);
                        preciosCreados++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Precios base copiados exitosamente",
                    preciosCreados
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al copiar precios base del viaje {ViajeId}", viajeId);
                return StatusCode(500, new { message = "Error al copiar los precios base" });
            }
        });
    }
}

