using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Boletos;
using prjBusTix.Model;
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
    private readonly ILogger<BoletosController> _logger;
    
    // Constantes para estatus
    private const int ESTATUS_BOLETO_PENDIENTE = 9;  // BOL_PENDIENTE
    private const int ESTATUS_BOLETO_PAGADO = 10;    // BOL_PAGADO
    private const int ESTATUS_PAGO_PENDIENTE = 14;   // PAG_PENDIENTE
    
    public BoletosController(AppDbContext context, ILogger<BoletosController> logger)
    {
        _context = context;
        _logger = logger;
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
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener boletos del usuario");
            return StatusCode(500, new { message = "Error al obtener los boletos" });
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

