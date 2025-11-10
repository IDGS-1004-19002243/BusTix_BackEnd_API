using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Pagos;
using prjBusTix.Model;
using prjBusTix.Services;
using System.Security.Claims;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PagosController> _logger;
    private readonly INotificacionService _notificacionService;
    
    // Constantes para estatus
    private const int ESTATUS_BOLETO_PENDIENTE = 9;  // BOL_PENDIENTE
    private const int ESTATUS_BOLETO_PAGADO = 10;    // BOL_PAGADO
    private const int ESTATUS_PAGO_PENDIENTE = 14;   // PAG_PENDIENTE
    private const int ESTATUS_PAGO_CAPTURADO = 15;   // PAG_CAPTURADO
    private const int ESTATUS_PAGO_RECHAZADO = 16;   // PAG_RECHAZADO
    private const int ESTATUS_ABORDAJE_PENDIENTE = 21; // ABD_PENDIENTE
    
    public PagosController(
        AppDbContext context, 
        ILogger<PagosController> logger,
        INotificacionService notificacionService)
    {
        _context = context;
        _logger = logger;
        _notificacionService = notificacionService;
    }
    
    /// <summary>
    /// Webhook o endpoint para confirmar pago desde pasarela
    /// POST /api/pagos/confirmacion
    /// </summary>
    [HttpPost("confirmacion")]
    [AllowAnonymous] // Permitir llamadas desde webhook
    public async Task<IActionResult> ConfirmarPago([FromBody] ConfirmacionPagoDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Buscar el pago por código
            var pago = await _context.Pagos
                .Include(p => p.PagosBoletos)
                    .ThenInclude(pb => pb.Boleto)
                        .ThenInclude(b => b.Viaje)
                .FirstOrDefaultAsync(p => p.CodigoPago == dto.CodigoPago);
            
            if (pago == null)
            {
                _logger.LogWarning("Pago no encontrado: {CodigoPago}", dto.CodigoPago);
                return NotFound(new { message = "Pago no encontrado" });
            }
            
            // Verificar que el pago está pendiente
            if (pago.Estatus != ESTATUS_PAGO_PENDIENTE)
            {
                _logger.LogWarning("Pago ya procesado: {CodigoPago}", dto.CodigoPago);
                return BadRequest(new { message = "El pago ya fue procesado" });
            }
            
            // Actualizar estado del pago según respuesta
            bool pagoExitoso = dto.Estado.ToLower() == "approved" || dto.Estado.ToLower() == "success";
            
            pago.TransaccionID = dto.TransaccionID;
            pago.Proveedor = dto.Proveedor ?? "Desconocido";
            pago.Estatus = pagoExitoso ? ESTATUS_PAGO_CAPTURADO : ESTATUS_PAGO_RECHAZADO;
            pago.FechaPago = DateTime.Now;
            
            if (dto.MontoConfirmado.HasValue && dto.MontoConfirmado.Value != pago.Monto)
            {
                _logger.LogWarning(
                    "Monto del pago no coincide. Esperado: {Esperado}, Recibido: {Recibido}",
                    pago.Monto, dto.MontoConfirmado.Value);
            }
            
            if (pagoExitoso)
            {
                // Actualizar boletos asociados
                foreach (var pagoBoleto in pago.PagosBoletos)
                {
                    var boleto = pagoBoleto.Boleto;
                    boleto.Estatus = ESTATUS_BOLETO_PAGADO;
                    
                    // Actualizar viaje
                    boleto.Viaje.AsientosVendidos++;
                    
                    // Crear entrada en ManifiestoPasajeros automáticamente
                    var manifiesto = new ManifiestoPasajero
                    {
                        ViajeID = boleto.ViajeID,
                        BoletoID = boleto.BoletoID,
                        NombreCompleto = boleto.NombrePasajero,
                        NumeroAsiento = boleto.NumeroAsiento,
                        ParadaAbordajeID = boleto.ParadaAbordajeID,
                        EstatusAbordaje = ESTATUS_ABORDAJE_PENDIENTE,
                        FueValidado = false
                    };
                    
                    _context.ManifiestoPasajeros.Add(manifiesto);
                    
                    _logger.LogInformation(
                        "Boleto {CodigoBoleto} actualizado a pagado y agregado al manifiesto",
                        boleto.CodigoBoleto);
                    
                    // 🔔 ENVIAR NOTIFICACIÓN AUTOMÁTICA DE CONFIRMACIÓN
                    await _notificacionService.EnviarConfirmacionCompraAsync(boleto.BoletoID);
                }
            }
            else
            {
                // Pago rechazado: liberar asientos
                foreach (var pagoBoleto in pago.PagosBoletos)
                {
                    var boleto = pagoBoleto.Boleto;
                    boleto.Viaje.AsientosDisponibles++;
                    
                    _logger.LogInformation(
                        "Boleto {CodigoBoleto} - Pago rechazado, asiento liberado",
                        boleto.CodigoBoleto);
                }
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            var response = new
            {
                success = pagoExitoso,
                message = pagoExitoso ? "Pago confirmado exitosamente" : "Pago rechazado",
                codigoPago = pago.CodigoPago,
                transaccionId = pago.TransaccionID,
                boletos = pago.PagosBoletos.Select(pb => new
                {
                    boletoId = pb.BoletoID,
                    codigoBoleto = pb.Boleto.CodigoBoleto,
                    estatus = pagoExitoso ? "Pagado" : "Pendiente"
                }).ToList()
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al confirmar pago {CodigoPago}", dto.CodigoPago);
            return StatusCode(500, new { message = "Error al procesar la confirmación del pago" });
        }
    }
    
    /// <summary>
    /// Obtiene información de un pago específico
    /// GET /api/pagos/{codigoPago}
    /// </summary>
    [HttpGet("{codigoPago}")]
    [Authorize]
    public async Task<ActionResult<PagoResponseDto>> ObtenerPago(string codigoPago)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            
            var pago = await _context.Pagos
                .Include(p => p.EstatusNavigation)
                .Include(p => p.PagosBoletos)
                .FirstOrDefaultAsync(p => p.CodigoPago == codigoPago);
            
            if (pago == null)
                return NotFound(new { message = "Pago no encontrado" });
            
            // Verificar que el pago pertenece al usuario o es admin
            if (!isAdmin && pago.UsuarioID != userId)
                return Forbid();
            
            var response = new PagoResponseDto
            {
                PagoID = pago.PagoID,
                CodigoPago = pago.CodigoPago,
                TransaccionID = pago.TransaccionID,
                Monto = pago.Monto,
                MetodoPago = pago.MetodoPago,
                Proveedor = pago.Proveedor,
                Estatus = pago.Estatus,
                EstatusNombre = pago.EstatusNavigation.Nombre,
                FechaPago = pago.FechaPago,
                BoletosIDs = pago.PagosBoletos.Select(pb => pb.BoletoID).ToList()
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pago {CodigoPago}", codigoPago);
            return StatusCode(500, new { message = "Error al obtener el pago" });
        }
    }
    
    /// <summary>
    /// Simula un pago exitoso (solo para desarrollo/testing)
    /// POST /api/pagos/simular-pago
    /// </summary>
    [HttpPost("simular-pago")]
    [Authorize]
    public async Task<IActionResult> SimularPago([FromBody] string codigoPago)
    {
        try
        {
            if (string.IsNullOrEmpty(codigoPago))
                return BadRequest(new { message = "Código de pago requerido" });
            
            var confirmacion = new ConfirmacionPagoDto
            {
                TransaccionID = $"SIM-{Guid.NewGuid().ToString("N")[..12].ToUpper()}",
                CodigoPago = codigoPago,
                Estado = "approved",
                Proveedor = "Simulado",
                MontoConfirmado = null
            };
            
            return await ConfirmarPago(confirmacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al simular pago");
            return StatusCode(500, new { message = "Error al simular el pago" });
        }
    }
    
    /// <summary>
    /// Obtiene el historial de pagos del usuario
    /// GET /api/pagos/me/historial
    /// </summary>
    [HttpGet("me/historial")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PagoResponseDto>>> MiHistorialPagos()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var pagos = await _context.Pagos
                .Include(p => p.EstatusNavigation)
                .Include(p => p.PagosBoletos)
                .Where(p => p.UsuarioID == userId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
            
            var response = pagos.Select(p => new PagoResponseDto
            {
                PagoID = p.PagoID,
                CodigoPago = p.CodigoPago,
                TransaccionID = p.TransaccionID,
                Monto = p.Monto,
                MetodoPago = p.MetodoPago,
                Proveedor = p.Proveedor,
                Estatus = p.Estatus,
                EstatusNombre = p.EstatusNavigation.Nombre,
                FechaPago = p.FechaPago,
                BoletosIDs = p.PagosBoletos.Select(pb => pb.BoletoID).ToList()
            }).ToList();
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de pagos");
            return StatusCode(500, new { message = "Error al obtener el historial" });
        }
    }
}
