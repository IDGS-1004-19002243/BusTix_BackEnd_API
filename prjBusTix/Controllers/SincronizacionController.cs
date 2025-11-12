using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Boletos;
using prjBusTix.Model;
using System.Security.Claims;

namespace prjBusTix.Controllers;

/// <summary>
/// Controlador para sincronización de validaciones offline
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Manager")]
public class SincronizacionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SincronizacionController> _logger;

    public SincronizacionController(AppDbContext context, ILogger<SincronizacionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Sincronizar validaciones offline en batch
    /// POST /api/sincronizacion/validaciones
    /// </summary>
    [HttpPost("validaciones")]
    public async Task<ActionResult<SincronizacionResponseDto>> SincronizarValidaciones(
        [FromBody] List<ValidacionSyncDto> validaciones)
    {
        if (validaciones == null || !validaciones.Any())
        {
            return BadRequest(new SincronizacionResponseDto
            {
                Success = false,
                Message = "No se enviaron validaciones para sincronizar",
                TotalRecibidas = 0
            });
        }

        var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = new SincronizacionResponseDto
        {
            TotalRecibidas = validaciones.Count,
            Procesadas = 0,
            Fallidas = 0,
            Duplicadas = 0,
            Errores = new List<ErrorValidacionDto>()
        };

        const int ESTATUS_BOLETO_PAGADO = 10;
        const int ESTATUS_BOLETO_USADO = 11;
        const int ESTATUS_ABORDAJE_ABORDADO = 22;

        foreach (var validacionDto in validaciones)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Verificar idempotencia
                var validacionExistente = await _context.RegistroValidacion
                    .FirstOrDefaultAsync(r => r.DeviceValidationId == validacionDto.DeviceValidationId);

                if (validacionExistente != null)
                {
                    response.Duplicadas++;
                    await transaction.RollbackAsync();
                    continue;
                }

                // Buscar boleto
                var boleto = await _context.Boletos
                    .Include(b => b.ManifiestoPasajero)
                    .FirstOrDefaultAsync(b => b.BoletoID == validacionDto.BoletoID && 
                                             b.ViajeID == validacionDto.ViajeID);

                if (boleto == null)
                {
                    response.Fallidas++;
                    response.Errores.Add(new ErrorValidacionDto
                    {
                        DeviceValidationId = validacionDto.DeviceValidationId,
                        BoletoID = validacionDto.BoletoID,
                        Error = "Boleto no encontrado"
                    });
                    await transaction.RollbackAsync();
                    continue;
                }

                // Verificar estado del boleto
                if (boleto.Estatus != ESTATUS_BOLETO_PAGADO && boleto.Estatus != ESTATUS_BOLETO_USADO)
                {
                    response.Fallidas++;
                    response.Errores.Add(new ErrorValidacionDto
                    {
                        DeviceValidationId = validacionDto.DeviceValidationId,
                        BoletoID = validacionDto.BoletoID,
                        Error = "El boleto no está en un estado válido"
                    });
                    await transaction.RollbackAsync();
                    continue;
                }

                // Solo actualizar si el resultado fue "Aprobado"
                if (validacionDto.Resultado == "Aprobado")
                {
                    // Actualizar boleto si no estaba usado
                    if (boleto.Estatus == ESTATUS_BOLETO_PAGADO)
                    {
                        boleto.Estatus = ESTATUS_BOLETO_USADO;
                        boleto.FechaValidacion = validacionDto.FechaHoraValidacion;
                        boleto.ValidadoPor = staffId;
                    }

                    // Actualizar manifiesto
                    if (boleto.ManifiestoPasajero != null)
                    {
                        boleto.ManifiestoPasajero.EstatusAbordaje = ESTATUS_ABORDAJE_ABORDADO;
                        boleto.ManifiestoPasajero.FechaAbordaje = validacionDto.FechaHoraValidacion;
                        boleto.ManifiestoPasajero.FueValidado = true;
                        boleto.ManifiestoPasajero.FechaValidacion = validacionDto.FechaHoraValidacion;
                        boleto.ManifiestoPasajero.ValidadoPor = staffId;
                    }
                }

                // Registrar validación en el histórico
                var registro = new RegistroValidacion
                {
                    BoletoID = validacionDto.BoletoID,
                    ViajeID = validacionDto.ViajeID,
                    ValidadoPor = staffId ?? string.Empty,
                    FechaHoraValidacion = validacionDto.FechaHoraValidacion,
                    CodigoQREscaneado = string.Empty, // No tenemos el QR completo en sync
                    ResultadoValidacion = validacionDto.Resultado,
                    TipoValidacion = validacionDto.TipoValidacion,
                    EstacionLat = validacionDto.EstacionLat,
                    EstacionLong = validacionDto.EstacionLong,
                    Observaciones = validacionDto.Observaciones,
                    ModoOffline = true, // Marca que fue sincronizado
                    DeviceValidationId = validacionDto.DeviceValidationId
                };

                _context.RegistroValidacion.Add(registro);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Procesadas++;

                _logger.LogInformation(
                    "Validación sincronizada: Boleto {BoletoID}, DeviceID {DeviceValidationId}", 
                    validacionDto.BoletoID, validacionDto.DeviceValidationId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Fallidas++;
                response.Errores.Add(new ErrorValidacionDto
                {
                    DeviceValidationId = validacionDto.DeviceValidationId,
                    BoletoID = validacionDto.BoletoID,
                    Error = $"Error al procesar: {ex.Message}"
                });

                _logger.LogError(ex, 
                    "Error sincronizando validación DeviceID {DeviceValidationId}", 
                    validacionDto.DeviceValidationId);
            }
        }

        response.Success = response.Fallidas == 0;
        response.Message = response.Success 
            ? "Sincronización completada exitosamente" 
            : $"Sincronización completada con {response.Fallidas} errores";

        _logger.LogInformation(
            "Sincronización completada: {Procesadas}/{Total} procesadas, {Duplicadas} duplicadas, {Fallidas} fallidas",
            response.Procesadas, response.TotalRecibidas, response.Duplicadas, response.Fallidas);

        return Ok(response);
    }
}

