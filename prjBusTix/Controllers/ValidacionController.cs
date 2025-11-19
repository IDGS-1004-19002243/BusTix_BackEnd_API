using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Boletos;
using System.Security.Claims;

namespace prjBusTix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public class ValidacionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ValidacionController> _logger;

        public ValidacionController(AppDbContext context, ILogger<ValidacionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Validación online de un boleto por escaneo de QR.
        /// POST /api/validacion
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ValidarBoletoResponseDto>> Validar([FromBody] ValidacionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidarBoletoResponseDto { EsValido = false, Mensaje = "Datos inválidos" });
            }

            const int ESTATUS_BOLETO_PAGADO = 10;   // BOL_PAGADO
            const int ESTATUS_BOLETO_VALIDADO = 11; // BOL_VALIDADO
            const int ESTATUS_BOLETO_USADO = 12;     // BOL_USADO
            const int ABD_ABORDADO = 22;             // ABD_ABORDADO

            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // Buscar boleto por QR y viaje
            var boleto = await _context.Boletos
                .Include(b => b.Viaje).ThenInclude(v => v.PlantillaRuta)
                .Include(b => b.ManifiestoPasajero)
                .FirstOrDefaultAsync(b => b.ViajeID == dto.ViajeID && b.CodigoQR == dto.CodigoQR);

            if (boleto == null)
            {
                // Registrar intento inválido
                _context.RegistroValidacion.Add(new prjBusTix.Model.RegistroValidacion
                {
                    ViajeID = dto.ViajeID,
                    BoletoID = 0,
                    ValidadoPor = staffId,
                    CodigoQREscaneado = dto.CodigoQR,
                    ResultadoValidacion = "Invalida",
                    TipoValidacion = dto.TipoValidacion,
                    EstacionLat = dto.EstacionLat,
                    EstacionLong = dto.EstacionLong,
                    Observaciones = dto.Observaciones,
                    ModoOffline = false,
                    FechaHoraValidacion = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Ok(new ValidarBoletoResponseDto
                {
                    EsValido = false,
                    Mensaje = "Boleto no encontrado para este viaje"
                });
            }

            // Duplicado si ya usado/abordado
            bool yaUsado = boleto.Estatus == ESTATUS_BOLETO_USADO || (boleto.ManifiestoPasajero?.FueValidado ?? false);
            if (yaUsado)
            {
                _context.RegistroValidacion.Add(new prjBusTix.Model.RegistroValidacion
                {
                    ViajeID = boleto.ViajeID,
                    BoletoID = boleto.BoletoID,
                    ValidadoPor = staffId,
                    CodigoQREscaneado = dto.CodigoQR,
                    ResultadoValidacion = "Duplicada",
                    TipoValidacion = dto.TipoValidacion,
                    EstacionLat = dto.EstacionLat,
                    EstacionLong = dto.EstacionLong,
                    Observaciones = dto.Observaciones,
                    ModoOffline = false,
                    FechaHoraValidacion = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Ok(new ValidarBoletoResponseDto
                {
                    EsValido = false,
                    Mensaje = "Boleto ya validado (duplicado)",
                    BoletoID = boleto.BoletoID,
                    NombrePasajero = boleto.NombrePasajero,
                    NumeroAsiento = boleto.NumeroAsiento,
                    CodigoViaje = boleto.Viaje.CodigoViaje,
                    CiudadOrigen = boleto.Viaje.PlantillaRuta.CiudadOrigen,
                    CiudadDestino = boleto.Viaje.PlantillaRuta.CiudadDestino,
                    FechaSalida = boleto.Viaje.FechaSalida,
                    FechaValidacion = boleto.FechaValidacion,
                    ValidadoPor = boleto.ValidadoPor
                });
            }

            // Verificar estado del boleto para abordar
            if (boleto.Estatus != ESTATUS_BOLETO_PAGADO && boleto.Estatus != ESTATUS_BOLETO_VALIDADO)
            {
                _context.RegistroValidacion.Add(new prjBusTix.Model.RegistroValidacion
                {
                    ViajeID = boleto.ViajeID,
                    BoletoID = boleto.BoletoID,
                    ValidadoPor = staffId,
                    CodigoQREscaneado = dto.CodigoQR,
                    ResultadoValidacion = "Invalida",
                    TipoValidacion = dto.TipoValidacion,
                    EstacionLat = dto.EstacionLat,
                    EstacionLong = dto.EstacionLong,
                    Observaciones = dto.Observaciones,
                    ModoOffline = false,
                    FechaHoraValidacion = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Ok(new ValidarBoletoResponseDto
                {
                    EsValido = false,
                    Mensaje = "El boleto no está en un estado válido para abordar",
                    BoletoID = boleto.BoletoID,
                    NombrePasajero = boleto.NombrePasajero,
                    NumeroAsiento = boleto.NumeroAsiento,
                    CodigoViaje = boleto.Viaje.CodigoViaje,
                    CiudadOrigen = boleto.Viaje.PlantillaRuta.CiudadOrigen,
                    CiudadDestino = boleto.Viaje.PlantillaRuta.CiudadDestino,
                    FechaSalida = boleto.Viaje.FechaSalida
                });
            }

            // Marcar como usado y actualizar manifiesto
            var now = DateTime.Now;
            boleto.Estatus = ESTATUS_BOLETO_USADO;
            boleto.FechaValidacion = now;
            boleto.ValidadoPor = staffId;

            if (boleto.ManifiestoPasajero != null)
            {
                boleto.ManifiestoPasajero.EstatusAbordaje = ABD_ABORDADO;
                boleto.ManifiestoPasajero.FechaAbordaje = now;
                boleto.ManifiestoPasajero.FueValidado = true;
                boleto.ManifiestoPasajero.FechaValidacion = now;
                boleto.ManifiestoPasajero.ValidadoPor = staffId;
            }

            _context.RegistroValidacion.Add(new prjBusTix.Model.RegistroValidacion
            {
                ViajeID = boleto.ViajeID,
                BoletoID = boleto.BoletoID,
                ValidadoPor = staffId,
                CodigoQREscaneado = dto.CodigoQR,
                ResultadoValidacion = "Exitosa",
                TipoValidacion = dto.TipoValidacion,
                EstacionLat = dto.EstacionLat,
                EstacionLong = dto.EstacionLong,
                Observaciones = dto.Observaciones,
                ModoOffline = false,
                FechaHoraValidacion = now
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Boleto {BoletoID} validado por {Staff}", boleto.BoletoID, staffId);

            return Ok(new ValidarBoletoResponseDto
            {
                EsValido = true,
                Mensaje = "Validación exitosa",
                BoletoID = boleto.BoletoID,
                NombrePasajero = boleto.NombrePasajero,
                NumeroAsiento = boleto.NumeroAsiento,
                CodigoViaje = boleto.Viaje.CodigoViaje,
                CiudadOrigen = boleto.Viaje.PlantillaRuta.CiudadOrigen,
                CiudadDestino = boleto.Viaje.PlantillaRuta.CiudadDestino,
                FechaSalida = boleto.Viaje.FechaSalida,
                FechaValidacion = boleto.FechaValidacion,
                ValidadoPor = staffId
            });
        }
    }
}
