using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Boletos;
using prjBusTix.Model;

namespace prjBusTix.Services;

public class ValidacionService : IValidacionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ValidacionService> _logger;

    public ValidacionService(AppDbContext context, ILogger<ValidacionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SincronizacionResponseDto> ProcesarValidacionesAsync(List<ValidacionSyncDto> validaciones, string staffId)
    {
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
                var validacionExistente = await _context.RegistroValidacion
                    .FirstOrDefaultAsync(r => r.DeviceValidationId == validacionDto.DeviceValidationId);

                if (validacionExistente != null)
                {
                    response.Duplicadas++;
                    await transaction.RollbackAsync();
                    continue;
                }

                var boleto = await _context.Boletos
                    .Include(b => b.ManifiestoPasajero)
                    .FirstOrDefaultAsync(b => b.BoletoID == validacionDto.BoletoID && b.ViajeID == validacionDto.ViajeID);

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

                if (validacionDto.Resultado == "Aprobado")
                {
                    if (boleto.Estatus == ESTATUS_BOLETO_PAGADO)
                    {
                        boleto.Estatus = ESTATUS_BOLETO_USADO;
                        boleto.FechaValidacion = validacionDto.FechaHoraValidacion;
                        boleto.ValidadoPor = staffId;
                    }

                    if (boleto.ManifiestoPasajero != null)
                    {
                        boleto.ManifiestoPasajero.EstatusAbordaje = ESTATUS_ABORDAJE_ABORDADO;
                        boleto.ManifiestoPasajero.FechaAbordaje = validacionDto.FechaHoraValidacion;
                        boleto.ManifiestoPasajero.FueValidado = true;
                        boleto.ManifiestoPasajero.FechaValidacion = validacionDto.FechaHoraValidacion;
                        boleto.ManifiestoPasajero.ValidadoPor = staffId;
                    }
                }

                var registro = new RegistroValidacion
                {
                    BoletoID = validacionDto.BoletoID,
                    ViajeID = validacionDto.ViajeID,
                    ValidadoPor = staffId ?? string.Empty,
                    FechaHoraValidacion = validacionDto.FechaHoraValidacion,
                    CodigoQREscaneado = string.Empty,
                    ResultadoValidacion = validacionDto.Resultado,
                    TipoValidacion = validacionDto.TipoValidacion,
                    EstacionLat = validacionDto.EstacionLat,
                    EstacionLong = validacionDto.EstacionLong,
                    Observaciones = validacionDto.Observaciones,
                    ModoOffline = true,
                    DeviceValidationId = validacionDto.DeviceValidationId
                };

                _context.RegistroValidacion.Add(registro);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Procesadas++;
                _logger.LogInformation("Validación sincronizada: Boleto {BoletoID}, DeviceID {DeviceValidationId}", validacionDto.BoletoID, validacionDto.DeviceValidationId);
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

                _logger.LogError(ex, "Error sincronizando validación DeviceID {DeviceValidationId}", validacionDto.DeviceValidationId);
            }
        }

        response.Success = response.Fallidas == 0;
        response.Message = response.Success ? "Sincronización completada exitosamente" : $"Sincronización completada con {response.Fallidas} errores";

        return response;
    }
}
