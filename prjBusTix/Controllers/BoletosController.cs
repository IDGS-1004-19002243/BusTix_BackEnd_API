    
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Boletos;
using prjBusTix.Model;
using prjBusTix.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoletosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly INotificacionService _notificacionService;
    private readonly ILogger<BoletosController> _logger;
    
    // Constantes para estatus
    private const int ESTATUS_BOLETO_PENDIENTE = 9;  // BOL_PENDIENTE
    private const int ESTATUS_BOLETO_PAGADO = 10;    // BOL_PAGADO
    private const int ESTATUS_PAGO_PENDIENTE = 14;   // PAG_PENDIENTE
    
    public BoletosController(
        AppDbContext context, 
        ILogger<BoletosController> logger,
        INotificacionService notificacionService)
    {
        _context = context;
        _logger = logger;
        _notificacionService = notificacionService;
    }
    
    /// <summary>
    /// Calcula el precio de un boleto antes de comprarlo
    /// GET /api/boletos/calcular-precio?viajeId=1&cuponId=1
    /// </summary>
    [HttpGet("calcular-precio")]
    public async Task<ActionResult<CalculoPrecioDto>> CalcularPrecio(
        [FromQuery] int viajeId, 
        [FromQuery] int? cuponId = null)
    {
        try
        {
            var viaje = await _context.Viajes
                .Include(v => v.PlantillaRuta)
                .Include(v => v.EstatusNavigation)
                .FirstOrDefaultAsync(v => v.ViajeID == viajeId);
            
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });
            
            if (!viaje.VentasAbiertas)
                return BadRequest(new { message = "Las ventas están cerradas para este viaje" });
            
            if (viaje.AsientosDisponibles <= 0)
                return BadRequest(new { message = "No hay asientos disponibles" });
            
            // Calcular precio
            decimal precioBase = viaje.PrecioBase;
            decimal cargoServicio = viaje.CargoServicio;
            decimal descuento = 0;
            decimal descuentoPorcentaje = 0;
            string? cuponAplicado = null;
            
            // Aplicar cupón si existe
            if (cuponId.HasValue)
            {
                var cupon = await _context.Cupones.FindAsync(cuponId.Value);
                if (cupon != null && cupon.EsActivo)
                {
                    // Validar vigencia
                    var ahora = DateTime.Now;
                    if (cupon.FechaInicio.HasValue && ahora < cupon.FechaInicio.Value)
                    {
                        return BadRequest(new { message = "El cupón aún no es válido" });
                    }
                    if (cupon.FechaExpiracion.HasValue && ahora > cupon.FechaExpiracion.Value)
                    {
                        return BadRequest(new { message = "El cupón ha expirado" });
                    }
                    
                    // Validar usos
                    if (cupon.UsosMaximos.HasValue && cupon.UsosRealizados >= cupon.UsosMaximos.Value)
                    {
                        return BadRequest(new { message = "El cupón ha alcanzado su límite de usos" });
                    }
                    
                    // Calcular descuento
                    if (cupon.TipoDescuento == "Porcentaje")
                    {
                        descuentoPorcentaje = cupon.ValorDescuento;
                        descuento = precioBase * (cupon.ValorDescuento / 100);
                    }
                    else if (cupon.TipoDescuento == "MontoFijo")
                    {
                        descuento = cupon.ValorDescuento;
                    }
                    
                    cuponAplicado = cupon.Codigo;
                }
            }
            
            // Cálculos finales
            decimal subtotal = precioBase + cargoServicio - descuento;
            decimal iva = subtotal * 0.16m; // 16% IVA
            decimal precioTotal = subtotal + iva;
            
            var resultado = new CalculoPrecioDto
            {
                ViajeID = viaje.ViajeID,
                CodigoViaje = viaje.CodigoViaje,
                CiudadOrigen = viaje.PlantillaRuta.CiudadOrigen,
                CiudadDestino = viaje.PlantillaRuta.CiudadDestino,
                FechaSalida = viaje.FechaSalida,
                PrecioBase = precioBase,
                CargoServicio = cargoServicio,
                Descuento = descuento,
                DescuentoPorcentaje = descuentoPorcentaje,
                Subtotal = subtotal,
                IVA = iva,
                PrecioTotal = precioTotal,
                CuponAplicado = cuponAplicado,
                VentasAbiertas = viaje.VentasAbiertas,
                AsientosDisponibles = viaje.AsientosDisponibles
            };
            
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular precio del boleto");
            return StatusCode(500, new { message = "Error al calcular el precio" });
        }
    }
    
    /// <summary>
    /// Inicia el proceso de compra de un boleto
    /// POST /api/boletos
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BoletoResponseDto>> IniciarCompra([FromBody] IniciarCompraDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });
            
            // Validar viaje
            var viaje = await _context.Viajes
                .Include(v => v.PlantillaRuta)
                .FirstOrDefaultAsync(v => v.ViajeID == dto.ViajeID);
            
            if (viaje == null)
                return NotFound(new { message = "Viaje no encontrado" });
            
            if (!viaje.VentasAbiertas)
                return BadRequest(new { message = "Las ventas están cerradas para este viaje" });
            
            if (viaje.AsientosDisponibles <= 0)
                return BadRequest(new { message = "No hay asientos disponibles" });
            
            // Calcular precio
            decimal precioBase = viaje.PrecioBase;
            decimal cargoServicio = viaje.CargoServicio;
            decimal descuento = 0;
            Cupon? cuponAplicado = null;
            
            if (dto.CuponID.HasValue)
            {
                cuponAplicado = await _context.Cupones.FindAsync(dto.CuponID.Value);
                if (cuponAplicado != null && cuponAplicado.EsActivo)
                {
                    if (cuponAplicado.TipoDescuento == "Porcentaje")
                        descuento = precioBase * (cuponAplicado.ValorDescuento / 100);
                    else if (cuponAplicado.TipoDescuento == "MontoFijo")
                        descuento = cuponAplicado.ValorDescuento;
                }
            }
            
            decimal subtotal = precioBase + cargoServicio - descuento;
            decimal iva = subtotal * 0.16m;
            decimal precioTotal = subtotal + iva;
            
            // Generar códigos únicos
            string codigoBoleto = GenerarCodigoBoleto();
            string codigoQR = GenerarCodigoQR(codigoBoleto);
            
            // Crear boleto
            var boleto = new Boleto
            {
                ViajeID = dto.ViajeID,
                ClienteID = userId,
                CodigoBoleto = codigoBoleto,
                CodigoQR = codigoQR,
                NumeroAsiento = dto.NumeroAsiento,
                NombrePasajero = dto.NombrePasajero,
                EmailPasajero = dto.EmailPasajero,
                TelefonoPasajero = dto.TelefonoPasajero,
                ParadaAbordajeID = dto.ParadaAbordajeID,
                PrecioBase = precioBase,
                Descuento = descuento,
                CargoServicio = cargoServicio,
                IVA = iva,
                PrecioTotal = precioTotal,
                CuponAplicadoID = dto.CuponID,
                FechaCompra = DateTime.Now,
                Estatus = ESTATUS_BOLETO_PENDIENTE
            };
            
            _context.Boletos.Add(boleto);
            await _context.SaveChangesAsync();
            
            // Actualizar asientos disponibles
            viaje.AsientosDisponibles--;
            await _context.SaveChangesAsync();
            
            // Crear pago pendiente
            string codigoPago = GenerarCodigoPago();
            var pago = new Pago
            {
                CodigoPago = codigoPago,
                UsuarioID = userId,
                Monto = precioTotal,
                MetodoPago = "Pendiente",
                FechaPago = DateTime.Now,
                Estatus = ESTATUS_PAGO_PENDIENTE
            };
            
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
            
            // Relacionar pago con boleto
            var pagoBoleto = new PagoBoleto
            {
                PagoID = pago.PagoID,
                BoletoID = boleto.BoletoID,
                MontoAsignado = precioTotal
            };
            
            _context.PagosBoletos.Add(pagoBoleto);
            
            // Actualizar usos del cupón
            if (cuponAplicado != null)
            {
                cuponAplicado.UsosRealizados++;
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // Preparar respuesta
            var estatusNombre = await _context.EstatusGenerales
                .Where(e => e.Id_Estatus == boleto.Estatus)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync() ?? "Pendiente";
            
            var response = new BoletoResponseDto
            {
                BoletoID = boleto.BoletoID,
                CodigoBoleto = boleto.CodigoBoleto,
                CodigoQR = boleto.CodigoQR,
                ViajeID = viaje.ViajeID,
                CodigoViaje = viaje.CodigoViaje,
                CiudadOrigen = viaje.PlantillaRuta.CiudadOrigen,
                CiudadDestino = viaje.PlantillaRuta.CiudadDestino,
                FechaSalida = viaje.FechaSalida,
                NumeroAsiento = boleto.NumeroAsiento,
                NombrePasajero = boleto.NombrePasajero,
                EmailPasajero = boleto.EmailPasajero,
                TelefonoPasajero = boleto.TelefonoPasajero,
                PrecioBase = boleto.PrecioBase,
                Descuento = boleto.Descuento,
                CargoServicio = boleto.CargoServicio,
                IVA = boleto.IVA,
                PrecioTotal = boleto.PrecioTotal,
                Estatus = boleto.Estatus,
                EstatusNombre = estatusNombre,
                FechaCompra = boleto.FechaCompra
            };
            
            return CreatedAtAction(nameof(ObtenerBoleto), new { id = boleto.BoletoID }, response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al crear boleto");
            return StatusCode(500, new { message = "Error al procesar la compra" });
        }
    }
    
    /// <summary>
    /// Obtiene un boleto específico
    /// GET /api/boletos/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BoletoResponseDto>> ObtenerBoleto(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            
            var boleto = await _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Include(b => b.ParadaAbordaje)
                .Include(b => b.EstatusNavigation)
                .FirstOrDefaultAsync(b => b.BoletoID == id);
            
            if (boleto == null)
                return NotFound(new { message = "Boleto no encontrado" });
            
            // Verificar que el boleto pertenece al usuario o es admin
            if (!isAdmin && boleto.ClienteID != userId)
                return Forbid();
            
            var response = MapBoletoToDto(boleto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener boleto {BoletoId}", id);
            return StatusCode(500, new { message = "Error al obtener el boleto" });
        }
    }
    
    /// <summary>
    /// Obtiene los boletos del usuario autenticado
    /// GET /api/boletos/me/boletos
    /// </summary>
    [HttpGet("me/boletos")]
    public async Task<ActionResult<IEnumerable<BoletoResponseDto>>> MisBoletos()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var boletos = await _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Include(b => b.ParadaAbordaje)
                .Include(b => b.EstatusNavigation)
                .Where(b => b.ClienteID == userId)
                .OrderByDescending(b => b.FechaCompra)
                .ToListAsync();
            
            var response = boletos.Select(MapBoletoToDto).ToList();
            return Ok(new
            {
                success = true,
                data = response,
                total = response.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener boletos del usuario");
            return StatusCode(500, new { 
                success = false,
                message = "Error al obtener los boletos" 
            });
        }
    }
    
    /// <summary>
    /// Valida un boleto por su código QR (usado por staff para validación)
    /// POST /api/boletos/validar-qr
    /// </summary>
    [HttpPost("validar-qr")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IActionResult> ValidarBoletoQR([FromBody] string codigoQR)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            if (string.IsNullOrWhiteSpace(codigoQR))
                return BadRequest(new { 
                    success = false,
                    message = "Código QR requerido" 
                });
            
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Buscar boleto por código QR
            var boleto = await _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Include(b => b.Cliente)
                .Include(b => b.EstatusNavigation)
                .FirstOrDefaultAsync(b => b.CodigoQR == codigoQR);
            
            if (boleto == null)
            {
                _logger.LogWarning("Código QR inválido escaneado: {CodigoQR}", codigoQR);
                return NotFound(new { 
                    success = false,
                    message = "Boleto no encontrado",
                    codigo = "QR_INVALIDO"
                });
            }
            
            // Validar que el boleto esté pagado
            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;
            const int ESTATUS_BOLETO_CANCELADO = 12;
            
            if (boleto.Estatus == ESTATUS_BOLETO_USADO)
            {
                return BadRequest(new { 
                    success = false,
                    message = "Este boleto ya fue usado",
                    codigo = "BOLETO_USADO",
                    fechaValidacion = boleto.FechaValidacion,
                    data = new {
                        codigoBoleto = boleto.CodigoBoleto,
                        pasajero = boleto.NombrePasajero,
                        viaje = $"{boleto.Viaje.PlantillaRuta.CiudadOrigen} → {boleto.Viaje.PlantillaRuta.CiudadDestino}"
                    }
                });
            }
            
            if (boleto.Estatus == ESTATUS_BOLETO_CANCELADO)
            {
                return BadRequest(new { 
                    success = false,
                    message = "Este boleto está cancelado",
                    codigo = "BOLETO_CANCELADO"
                });
            }
            
            if (boleto.Estatus != ESTATUS_BOLETO_PAGADO)
            {
                return BadRequest(new { 
                    success = false,
                    message = "El boleto no está en estado válido para usar",
                    codigo = "ESTATUS_INVALIDO",
                    estatusActual = boleto.EstatusNavigation.Nombre
                });
            }
            
            // Validar que el viaje no haya pasado
            if (boleto.Viaje.FechaSalida < DateTime.Now.AddHours(-6)) // Tolerancia de 6 horas
            {
                return BadRequest(new { 
                    success = false,
                    message = "El viaje ya pasó",
                    codigo = "VIAJE_EXPIRADO",
                    fechaSalida = boleto.Viaje.FechaSalida
                });
            }
            
            // Marcar boleto como usado y validado
            boleto.Estatus = ESTATUS_BOLETO_USADO;
            boleto.FechaValidacion = DateTime.Now;
            
            // Crear registro de validación
            var registroValidacion = new RegistroValidacion
            {
                BoletoID = boleto.BoletoID,
                ViajeID = boleto.ViajeID,
                ValidadoPor = staffId ?? string.Empty,
                CodigoQREscaneado = codigoQR,
                ResultadoValidacion = "Exitosa",
                FechaHoraValidacion = DateTime.Now,
                ModoOffline = false
            };
            
            _context.RegistroValidacion.Add(registroValidacion);
            
            // Actualizar manifiesto de pasajeros
            var manifiesto = await _context.ManifiestoPasajeros
                .FirstOrDefaultAsync(m => m.BoletoID == boleto.BoletoID);
            
            if (manifiesto != null)
            {
                const int ESTATUS_ABORDAJE_ABORDADO = 22; // ABD_ABORDADO
                manifiesto.EstatusAbordaje = ESTATUS_ABORDAJE_ABORDADO;
                manifiesto.FueValidado = true;
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            _logger.LogInformation(
                "Boleto {CodigoBoleto} validado por staff {StaffId}",
                boleto.CodigoBoleto, staffId);
            
            return Ok(new {
                success = true,
                message = "✅ Boleto validado correctamente",
                codigo = "VALIDACION_EXITOSA",
                data = new {
                    codigoBoleto = boleto.CodigoBoleto,
                    pasajero = boleto.NombrePasajero,
                    email = boleto.EmailPasajero,
                    telefono = boleto.TelefonoPasajero,
                    asiento = boleto.NumeroAsiento,
                    viaje = new {
                        codigo = boleto.Viaje.CodigoViaje,
                        origen = boleto.Viaje.PlantillaRuta.CiudadOrigen,
                        destino = boleto.Viaje.PlantillaRuta.CiudadDestino,
                        fechaSalida = boleto.Viaje.FechaSalida
                    },
                    precioTotal = boleto.PrecioTotal,
                    fechaValidacion = boleto.FechaValidacion
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al validar boleto QR");
            return StatusCode(500, new { 
                success = false,
                message = "Error al validar el boleto",
                codigo = "ERROR_SISTEMA"
            });
        }
    }
    
    /// <summary>
    /// Verifica la validez de un boleto sin marcarlo como usado
    /// GET /api/boletos/verificar/{codigoBoleto}
    /// </summary>
    [HttpGet("verificar/{codigoBoleto}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<ActionResult> VerificarBoleto(string codigoBoleto)
    {
        try
        {
            var boleto = await _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Include(b => b.Cliente)
                .Include(b => b.EstatusNavigation)
                .FirstOrDefaultAsync(b => b.CodigoBoleto == codigoBoleto);
            
            if (boleto == null)
            {
                return NotFound(new { 
                    success = false,
                    message = "Boleto no encontrado"
                });
            }
            
            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;
            
            bool esValido = boleto.Estatus == ESTATUS_BOLETO_PAGADO && 
                           boleto.Viaje.FechaSalida > DateTime.Now.AddHours(-6);
            
            return Ok(new {
                success = true,
                esValido,
                data = new {
                    codigoBoleto = boleto.CodigoBoleto,
                    pasajero = boleto.NombrePasajero,
                    asiento = boleto.NumeroAsiento,
                    estatus = boleto.EstatusNavigation.Nombre,
                    estatusId = boleto.Estatus,
                    yaUsado = boleto.Estatus == ESTATUS_BOLETO_USADO,
                    fechaValidacion = boleto.FechaValidacion,
                    viaje = new {
                        codigo = boleto.Viaje.CodigoViaje,
                        origen = boleto.Viaje.PlantillaRuta.CiudadOrigen,
                        destino = boleto.Viaje.PlantillaRuta.CiudadDestino,
                        fechaSalida = boleto.Viaje.FechaSalida
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar boleto {CodigoBoleto}", codigoBoleto);
            return StatusCode(500, new { 
                success = false,
                message = "Error al verificar el boleto" 
            });
        }
    }
    
    /// <summary>
    /// Cancelar un boleto y procesar reembolso
    /// PUT /api/boletos/{id}/cancelar
    /// </summary>
    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> CancelarBoleto(int id, [FromBody] CancelarBoletoDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var boleto = await _context.Boletos
                .Include(b => b.Viaje)
                .Include(b => b.PagosBoletos)
                    .ThenInclude(pb => pb.Pago)
                .FirstOrDefaultAsync(b => b.BoletoID == id);
            
            if (boleto == null)
                return NotFound(new { success = false, message = "Boleto no encontrado" });
            
            // Validar que el boleto pertenece al usuario (o es admin)
            if (boleto.ClienteID != userId && !User.IsInRole("Admin"))
                return Forbid();
            
            const int ESTATUS_BOLETO_CANCELADO = 12;
            const int ESTATUS_BOLETO_USADO = 11;
            
            // No se puede cancelar un boleto ya usado
            if (boleto.Estatus == ESTATUS_BOLETO_USADO)
                return BadRequest(new { success = false, message = "No se puede cancelar un boleto ya usado" });
            
            // No se puede cancelar si ya está cancelado
            if (boleto.Estatus == ESTATUS_BOLETO_CANCELADO)
                return BadRequest(new { success = false, message = "El boleto ya está cancelado" });
            
            // Validar tiempo de cancelación (ej: 24 horas antes del viaje)
            var horasAntes = (boleto.Viaje.FechaSalida - DateTime.Now).TotalHours;
            if (horasAntes < 24 && !User.IsInRole("Admin"))
                return BadRequest(new { 
                    success = false, 
                    message = "Solo se pueden cancelar boletos con 24 horas de anticipación" 
                });
            
            // Actualizar estatus del boleto
            boleto.Estatus = ESTATUS_BOLETO_CANCELADO;
            
            // Liberar asiento
            boleto.Viaje.AsientosDisponibles++;
            boleto.Viaje.AsientosVendidos--;
            
            // Actualizar manifiesto si existe
            var manifiesto = await _context.ManifiestoPasajeros
                .FirstOrDefaultAsync(m => m.BoletoID == id);
            
            if (manifiesto != null)
            {
                manifiesto.EstatusAbordaje = 23; // ABD_CANCELADO (asumiendo este ID)
            }
            
            // Procesar reembolso (simulado - aquí irían las llamadas a la pasarela)
            decimal montoReembolso = boleto.PrecioTotal;
            
            // Aplicar penalidad si corresponde
            if (horasAntes < 48 && horasAntes >= 24)
            {
                montoReembolso *= 0.8m; // 80% de reembolso
            }
            
            // Registrar el reembolso en el pago
            var pagoBoleto = boleto.PagosBoletos.FirstOrDefault();
            if (pagoBoleto != null)
            {
                pagoBoleto.Pago.Estatus = 17; // PAG_REEMBOLSADO (asumiendo este ID)
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // Enviar notificación
            try
            {
                var notificacion = new Notificacion
                {
                    UsuarioID = userId ?? string.Empty,
                    Titulo = "Boleto Cancelado",
                    Mensaje = $"Tu boleto {boleto.CodigoBoleto} ha sido cancelado. Reembolso: ${montoReembolso:F2}",
                    TipoNotificacion = "CANCELACION",
                    FechaCreacion = DateTime.Now,
                    FueLeida = false
                };
                
                await _notificacionService.EnviarNotificacionAsync(notificacion);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar notificación de cancelación");
            }
            
            _logger.LogInformation("Boleto {BoletoID} cancelado por usuario {UserId}", id, userId);
            
            return Ok(new {
                success = true,
                message = "Boleto cancelado correctamente",
                montoReembolso,
                porcentajeReembolso = horasAntes < 48 ? 80 : 100,
                tiempoEstimadoReembolso = "3-5 días hábiles"
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al cancelar boleto {BoletoID}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Error al cancelar el boleto" 
            });
        }
    }
    
    /// <summary>
    /// Cambiar asiento de un boleto
    /// PUT /api/boletos/{id}/cambiar-asiento
    /// </summary>
    [HttpPut("{id}/cambiar-asiento")]
    public async Task<IActionResult> CambiarAsiento(int id, [FromBody] CambiarAsientoDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var boleto = await _context.Boletos
                .Include(b => b.Viaje)
                .FirstOrDefaultAsync(b => b.BoletoID == id);
            
            if (boleto == null)
                return NotFound(new { success = false, message = "Boleto no encontrado" });
            
            // Validar que el boleto pertenece al usuario
            if (boleto.ClienteID != userId && !User.IsInRole("Admin"))
                return Forbid();
            
            const int ESTATUS_BOLETO_PAGADO = 10;
            
            // Solo se puede cambiar asiento si el boleto está pagado
            if (boleto.Estatus != ESTATUS_BOLETO_PAGADO)
                return BadRequest(new { 
                    success = false, 
                    message = "Solo se pueden cambiar asientos de boletos pagados" 
                });
            
            // Validar que el viaje no haya salido
            if (boleto.Viaje.FechaSalida <= DateTime.Now)
                return BadRequest(new { 
                    success = false, 
                    message = "No se puede cambiar el asiento de un viaje que ya salió" 
                });
            
            // Validar que el nuevo asiento no esté ocupado
            var asientoOcupado = await _context.Boletos
                .AnyAsync(b => b.ViajeID == boleto.ViajeID && 
                              b.NumeroAsiento == dto.NuevoAsiento && 
                              b.BoletoID != id &&
                              b.Estatus == ESTATUS_BOLETO_PAGADO);
            
            if (asientoOcupado)
                return BadRequest(new { 
                    success = false, 
                    message = "El asiento seleccionado ya está ocupado" 
                });
            
            var asientoAnterior = boleto.NumeroAsiento;
            boleto.NumeroAsiento = dto.NuevoAsiento;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Asiento cambiado para boleto {BoletoID}: {AsientoAnterior} → {AsientoNuevo}", 
                id, asientoAnterior, dto.NuevoAsiento);
            
            return Ok(new {
                success = true,
                message = "Asiento cambiado correctamente",
                asientoAnterior,
                asientoNuevo = dto.NuevoAsiento
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar asiento del boleto {BoletoID}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Error al cambiar el asiento" 
            });
        }
    }
    
    /// <summary>
    /// Validar boleto por ID (para staff con geolocalización)
    /// POST /api/boletos/{id}/validar
    /// </summary>
    [HttpPost("{id}/validar")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IActionResult> ValidarBoleto(int id, [FromBody] ValidarBoletoDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var boleto = await _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Include(b => b.Cliente)
                .Include(b => b.EstatusNavigation)
                .Include(b => b.ManifiestoPasajero)
                .FirstOrDefaultAsync(b => b.BoletoID == id && b.CodigoQR == dto.CodigoQR);
            
            if (boleto == null)
                return NotFound(new { 
                    success = false, 
                    message = "Boleto no encontrado o código QR no coincide" 
                });
            
            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;
            const int ESTATUS_BOLETO_CANCELADO = 12;
            const int ESTATUS_ABORDAJE_ABORDADO = 22; // ABD_ABORDADO
            
            // Validaciones
            if (boleto.Estatus == ESTATUS_BOLETO_USADO)
                return BadRequest(new { 
                    success = false, 
                    message = "Este boleto ya fue validado anteriormente",
                    fechaValidacion = boleto.FechaValidacion
                });
            
            if (boleto.Estatus == ESTATUS_BOLETO_CANCELADO)
                return BadRequest(new { 
                    success = false, 
                    message = "Este boleto está cancelado" 
                });
            
            if (boleto.Estatus != ESTATUS_BOLETO_PAGADO)
                return BadRequest(new { 
                    success = false, 
                    message = "El boleto no está en estado válido para usar" 
                });
            
            // Actualizar boleto
            boleto.Estatus = ESTATUS_BOLETO_USADO;
            boleto.FechaValidacion = DateTime.Now;
            boleto.ValidadoPor = staffId;
            
            // Actualizar manifiesto
            if (boleto.ManifiestoPasajero != null)
            {
                boleto.ManifiestoPasajero.EstatusAbordaje = ESTATUS_ABORDAJE_ABORDADO;
                boleto.ManifiestoPasajero.FechaAbordaje = DateTime.Now;
                boleto.ManifiestoPasajero.FueValidado = true;
                boleto.ManifiestoPasajero.FechaValidacion = DateTime.Now;
                boleto.ManifiestoPasajero.ValidadoPor = staffId;
            }
            
            // Registrar validación
            var registro = new RegistroValidacion
            {
                BoletoID = boleto.BoletoID,
                ViajeID = boleto.ViajeID,
                ValidadoPor = staffId ?? string.Empty,
                FechaHoraValidacion = DateTime.Now,
                CodigoQREscaneado = dto.CodigoQR,
                ResultadoValidacion = "Exitosa",
                ModoOffline = false
            };
            
            _context.RegistroValidacion.Add(registro);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            var response = new ValidarBoletoResponseDto
            {
                EsValido = true,
                Mensaje = "Boleto validado correctamente",
                BoletoID = boleto.BoletoID,
                NombrePasajero = boleto.NombrePasajero,
                NumeroAsiento = boleto.NumeroAsiento,
                CodigoViaje = boleto.Viaje.CodigoViaje,
                CiudadOrigen = boleto.Viaje.PlantillaRuta.CiudadOrigen,
                CiudadDestino = boleto.Viaje.PlantillaRuta.CiudadDestino,
                FechaSalida = boleto.Viaje.FechaSalida,
                FechaValidacion = boleto.FechaValidacion,
                ValidadoPor = staffId
            };
            
            _logger.LogInformation(
                "Boleto {BoletoID} validado por staff {StaffId}", 
                id, staffId);
            
            return Ok(new { success = true, data = response });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al validar boleto {BoletoID}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Error al validar el boleto" 
            });
        }
    }
    // Métodos auxiliares
    private string GenerarCodigoBoleto()
    {
        return $"BOL-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
    
    private string GenerarCodigoQR(string codigoBoleto)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{codigoBoleto}-{DateTime.Now.Ticks}"));
        return Convert.ToBase64String(hash);
    }
    
    private string GenerarCodigoPago()
    {
        return $"PAG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
    
    private BoletoResponseDto MapBoletoToDto(Boleto boleto)
    {
        return new BoletoResponseDto
        {
            BoletoID = boleto.BoletoID,
            CodigoBoleto = boleto.CodigoBoleto,
            CodigoQR = boleto.CodigoQR,
            ViajeID = boleto.ViajeID,
            CodigoViaje = boleto.Viaje.CodigoViaje,
            CiudadOrigen = boleto.Viaje.PlantillaRuta.CiudadOrigen,
            CiudadDestino = boleto.Viaje.PlantillaRuta.CiudadDestino,
            FechaSalida = boleto.Viaje.FechaSalida,
            NumeroAsiento = boleto.NumeroAsiento,
            NombrePasajero = boleto.NombrePasajero,
            EmailPasajero = boleto.EmailPasajero,
            TelefonoPasajero = boleto.TelefonoPasajero,
            PrecioBase = boleto.PrecioBase,
            Descuento = boleto.Descuento,
            CargoServicio = boleto.CargoServicio,
            IVA = boleto.IVA,
            PrecioTotal = boleto.PrecioTotal,
            Estatus = boleto.Estatus,
            EstatusNombre = boleto.EstatusNavigation.Nombre,
            FechaCompra = boleto.FechaCompra,
            FechaValidacion = boleto.FechaValidacion,
            ParadaAbordaje = boleto.ParadaAbordaje?.NombreParada,
            HoraEstimadaAbordaje = boleto.ParadaAbordaje?.HoraEstimadaLlegada
        };
    }
}

