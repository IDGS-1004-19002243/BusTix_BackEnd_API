using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Security;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/reportes/financieros")]
[Authorize]
public class ReportesFinancierosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportesFinancierosController> _logger;

    // Constantes y configuración local
    private static class Statuses
    {
        public const int BoletosPagado = 10;
        public const int BoletosUsado = 11;
    }

    public ReportesFinancierosController(AppDbContext context, ILogger<ReportesFinancierosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Helper: normaliza rango de fechas (UTC) y asegura desde <= hasta
    private static (DateTime desdeUtc, DateTime hastaUtc) NormalizeRange(DateTime? desde, DateTime? hasta)
    {
        var now = DateTime.UtcNow;
        var d = (desde ?? now.AddMonths(-1)).ToUniversalTime();
        var h = (hasta ?? now).ToUniversalTime();

        // Asegurar que hasta incluya el final del día si la hora no fue especificada
        if (hasta.HasValue && hasta.Value.TimeOfDay == TimeSpan.Zero)
        {
            h = h.Date.AddDays(1).AddTicks(-1);
        }

        if (d > h)
        {
            var tmp = d; d = h; h = tmp;
        }

        return (d, h);
    }

    /// <summary>
    /// Reporte de pagos: totales, por proveedor, por método y por estatus
    /// </summary>
    /// <response code="200">Reporte generado correctamente</response>
    [HttpGet("pagos")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetReportePagos(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] string? proveedor = null,
        [FromQuery] string? metodo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (desdeUtc, hastaUtc) = NormalizeRange(fechaDesde, fechaHasta);

            var query = _context.Pagos.AsNoTracking()
                .Where(p => p.FechaPago >= desdeUtc && p.FechaPago <= hastaUtc);

            if (!string.IsNullOrWhiteSpace(proveedor))
                query = query.Where(p => p.Proveedor == proveedor);

            if (!string.IsNullOrWhiteSpace(metodo))
                query = query.Where(p => p.MetodoPago == metodo);

            var totalPagos = await query.CountAsync(cancellationToken);
            var montoTotal = await query.SumAsync(p => (decimal?)p.Monto, cancellationToken) ?? 0m;

            var porProveedor = await query
                .GroupBy(p => p.Proveedor)
                .Select(g => new { proveedor = g.Key ?? "(sin proveedor)", total = g.Sum(p => (decimal?)p.Monto) ?? 0m, count = g.Count() })
                .OrderByDescending(x => x.total)
                .ToListAsync(cancellationToken);

            var porMetodo = await query
                .GroupBy(p => p.MetodoPago)
                .Select(g => new { metodo = g.Key ?? "(sin metodo)", total = g.Sum(p => (decimal?)p.Monto) ?? 0m, count = g.Count() })
                .OrderByDescending(x => x.total)
                .ToListAsync(cancellationToken);

            var porEstatus = await query
                .GroupBy(p => p.Estatus)
                .Select(g => new { estatus = g.Key, total = g.Sum(p => (decimal?)p.Monto) ?? 0m, count = g.Count() })
                .OrderByDescending(x => x.total)
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                desde = desdeUtc,
                hasta = hastaUtc,
                totalPagos,
                montoTotal,
                porProveedor,
                porMetodo,
                porEstatus
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Solicitud de reporte de pagos cancelada por el cliente");
            return BadRequest(new { message = "Request canceled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de pagos");
            return StatusCode(500, new { message = "Error al generar reporte de pagos", error = ex.Message });
        }
    }

    /// <summary>
    /// Reporte de ingresos (agrupa por día/mes/ruta)
    /// </summary>
    [HttpGet("ingresos")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetReporteIngresos(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] string? agruparPor = "dia",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (desdeUtc, hastaUtc) = NormalizeRange(fechaDesde, fechaHasta);

            var query = _context.Boletos.AsNoTracking()
                .Include(b => b.Viaje)
                .ThenInclude(v => v.PlantillaRuta)
                .Where(b => b.FechaCompra >= desdeUtc && b.FechaCompra <= hastaUtc &&
                       (b.Estatus == Statuses.BoletosPagado || b.Estatus == Statuses.BoletosUsado));

            var ingresoTotal = await query.SumAsync(b => (decimal?)b.PrecioTotal, cancellationToken) ?? 0m;
            var ingresoBase = await query.SumAsync(b => (decimal?)b.PrecioBase, cancellationToken) ?? 0m;
            var descuentos = await query.SumAsync(b => (decimal?)b.Descuento, cancellationToken) ?? 0m;
            var cargos = await query.SumAsync(b => (decimal?)b.CargoServicio, cancellationToken) ?? 0m;

            object agrupado;

            if (string.Equals(agruparPor, "mes", StringComparison.OrdinalIgnoreCase))
            {
                agrupado = await query
                    .GroupBy(b => new { year = b.FechaCompra.Year, month = b.FechaCompra.Month })
                    .Select(g => new
                    {
                        year = g.Key.year,
                        month = g.Key.month,
                        ingreso = g.Sum(b => (decimal?)b.PrecioTotal) ?? 0m,
                        boletos = g.Count()
                    })
                    .OrderBy(x => x.year).ThenBy(x => x.month)
                    .ToListAsync(cancellationToken);
            }
            else if (string.Equals(agruparPor, "ruta", StringComparison.OrdinalIgnoreCase))
            {
                agrupado = await query
                    .GroupBy(b => new { rutaId = b.Viaje != null ? b.Viaje.PlantillaRutaID : (int?)null, rutaNombre = b.Viaje != null && b.Viaje.PlantillaRuta != null ? b.Viaje.PlantillaRuta.NombreRuta : "(sin ruta)" })
                    .Select(g => new
                    {
                        rutaId = g.Key.rutaId,
                        rutaNombre = g.Key.rutaNombre,
                        ingreso = g.Sum(b => (decimal?)b.PrecioTotal) ?? 0m,
                        boletos = g.Count()
                    })
                    .OrderByDescending(x => x.ingreso)
                    .ToListAsync(cancellationToken);
            }
            else // por dia por defecto
            {
                agrupado = await query
                    .GroupBy(b => b.FechaCompra.Date)
                    .Select(g => new
                    {
                        fecha = g.Key,
                        ingreso = g.Sum(b => (decimal?)b.PrecioTotal) ?? 0m,
                        boletos = g.Count()
                    })
                    .OrderBy(x => x.fecha)
                    .ToListAsync(cancellationToken);
            }

            return Ok(new
            {
                desde = desdeUtc,
                hasta = hastaUtc,
                ingresoTotal,
                ingresoBase,
                descuentos,
                cargos,
                agrupado
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Solicitud de reporte de ingresos cancelada por el cliente");
            return BadRequest(new { message = "Request canceled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ingresos");
            return StatusCode(500, new { message = "Error al generar reporte de ingresos", error = ex.Message });
        }
    }

    /// <summary>
    /// Reporte financiero de ventas (ticket promedio, desglose de comisiones/cargos/descuentos)
    /// </summary>
    [HttpGet("ventas")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetReporteVentasFinancieras(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (desdeUtc, hastaUtc) = NormalizeRange(fechaDesde, fechaHasta);

            var query = _context.Boletos.AsNoTracking()
                .Include(b => b.Viaje)
                .Where(b => b.FechaCompra >= desdeUtc && b.FechaCompra <= hastaUtc && (b.Estatus == Statuses.BoletosPagado || b.Estatus == Statuses.BoletosUsado));

            var totalBoletos = await query.CountAsync(cancellationToken);
            var ingresoTotal = await query.SumAsync(b => (decimal?)b.PrecioTotal, cancellationToken) ?? 0m;
            var ingresoBase = await query.SumAsync(b => (decimal?)b.PrecioBase, cancellationToken) ?? 0m;
            var descuentos = await query.SumAsync(b => (decimal?)b.Descuento, cancellationToken) ?? 0m;
            var cargos = await query.SumAsync(b => (decimal?)b.CargoServicio, cancellationToken) ?? 0m;

            var ticketPromedio = totalBoletos > 0 ? Math.Round(ingresoTotal / totalBoletos, 2) : 0m;

            // Ventas por ruta - proteger nulos
            var ventasPorRuta = await query
                .GroupBy(b => b.Viaje != null ? b.Viaje.PlantillaRutaID : (int?)null)
                .Select(g => new
                {
                    rutaId = g.Key,
                    ingresos = g.Sum(b => (decimal?)b.PrecioTotal) ?? 0m,
                    boletos = g.Count()
                })
                .OrderByDescending(x => x.ingresos)
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                desde = desdeUtc,
                hasta = hastaUtc,
                totalBoletos,
                ingresoTotal,
                ingresoBase,
                descuentos,
                cargos,
                ticketPromedio,
                ventasPorRuta
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Solicitud de reporte de ventas cancelada por el cliente");
            return BadRequest(new { message = "Request canceled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ventas financieras");
            return StatusCode(500, new { message = "Error al generar reporte de ventas financieras", error = ex.Message });
        }
    }
}
