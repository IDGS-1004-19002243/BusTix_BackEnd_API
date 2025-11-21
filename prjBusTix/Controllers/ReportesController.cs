using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Security;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(AppDbContext context, ILogger<ReportesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Reporte de ventas por período (Optimizado)
    /// GET /api/reportes/ventas
    /// </summary>
    [HttpGet("ventas")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public async Task<ActionResult> GetReporteVentas(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int? eventoId = null)
    {
        try
        {
            var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
            var hasta = fechaHasta ?? DateTime.Now;

            var query = _context.Boletos.AsQueryable();

            // Filtros
            query = query.Where(b => b.FechaCompra >= desde && b.FechaCompra <= hasta);

            if (eventoId.HasValue)
                query = query.Where(b => b.Viaje.EventoID == eventoId.Value);

            // 1. Totales Generales (Ejecutado en BD)
            var stats = await query
                .GroupBy(b => 1)
                .Select(g => new
                {
                    TotalBoletos = g.Count(),
                    BoletosVendidos = g.Count(b => b.Estatus == 10 || b.Estatus == 11),
                    BoletosCancelados = g.Count(b => b.Estatus == 12),
                    IngresoTotal = g.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => (decimal?)b.PrecioTotal) ?? 0,
                    IngresoBase = g.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => (decimal?)b.PrecioBase) ?? 0,
                    Descuentos = g.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => (decimal?)b.Descuento) ?? 0,
                    Cargos = g.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => (decimal?)b.CargoServicio) ?? 0
                })
                .FirstOrDefaultAsync();

            // 2. Ventas por Evento (Ejecutado en BD)
            var ventasPorEvento = await query
                .GroupBy(b => new { b.Viaje.EventoID, b.Viaje.Evento.Nombre })
                .Select(g => new
                {
                    eventoId = g.Key.EventoID,
                    eventoNombre = g.Key.Nombre,
                    totalBoletos = g.Count(),
                    ingresoTotal = g.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => (decimal?)b.PrecioTotal) ?? 0
                })
                .OrderByDescending(x => x.ingresoTotal)
                .ToListAsync();

            // 3. Ventas por Día (Ejecutado en BD)
            var ventasPorDia = await query
                .GroupBy(b => b.FechaCompra.Date)
                .Select(g => new
                {
                    fecha = g.Key,
                    totalBoletos = g.Count(),
                    ingresoTotal = g.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => (decimal?)b.PrecioTotal) ?? 0
                })
                .OrderBy(x => x.fecha)
                .ToListAsync();

            var reporte = new
            {
                periodo = new { desde, hasta },
                totalBoletos = stats?.TotalBoletos ?? 0,
                boletosVendidos = stats?.BoletosVendidos ?? 0,
                boletosCancelados = stats?.BoletosCancelados ?? 0,
                ingresoTotal = stats?.IngresoTotal ?? 0,
                ingresoBase = stats?.IngresoBase ?? 0,
                descuentosAplicados = stats?.Descuentos ?? 0,
                cargosServicio = stats?.Cargos ?? 0,
                ventasPorEvento,
                ventasPorDia
            };

            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ventas");
            return StatusCode(500, new { message = "Error al generar reporte", error = ex.Message });
        }
    }

    /// <summary>
    /// Reporte de ocupación de viajes (Optimizado)
    /// GET /api/reportes/ocupacion
    /// </summary>
    [HttpGet("ocupacion")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public async Task<ActionResult> GetReporteOcupacion(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int? eventoId = null)
    {
        try
        {
            var desde = fechaDesde ?? DateTime.Now;
            var hasta = fechaHasta ?? DateTime.Now.AddMonths(1);

            var query = _context.Viajes.AsQueryable();

            // Filtros
            query = query.Where(v => v.FechaSalida >= desde && v.FechaSalida <= hasta);

            if (eventoId.HasValue)
                query = query.Where(v => v.EventoID == eventoId.Value);

            // 1. Totales Generales (BD)
            var stats = await query
                .GroupBy(v => 1)
                .Select(g => new
                {
                    TotalViajes = g.Count(),
                    ViajesCompletos = g.Count(v => v.AsientosDisponibles == 0),
                    TotalAsientos = g.Sum(v => v.CupoTotal),
                    TotalVendidos = g.Sum(v => v.AsientosVendidos)
                })
                .FirstOrDefaultAsync();

            // 2. Ocupación por Evento (BD)
            var rawOcupacionEvento = await query
                .GroupBy(v => new { v.EventoID, v.Evento.Nombre })
                .Select(g => new
                {
                    eventoId = g.Key.EventoID,
                    eventoNombre = g.Key.Nombre,
                    totalViajes = g.Count(),
                    totalAsientos = g.Sum(v => v.CupoTotal),
                    asientosVendidos = g.Sum(v => v.AsientosVendidos)
                })
                .ToListAsync();

            // Calcular porcentajes en memoria (más seguro y limpio)
            var ocupacionPorEvento = rawOcupacionEvento
                .Select(x => new
                {
                    x.eventoId,
                    x.eventoNombre,
                    x.totalViajes,
                    x.totalAsientos,
                    x.asientosVendidos,
                    porcentajeOcupacion = x.totalAsientos > 0 
                        ? Math.Round((x.asientosVendidos * 100.0) / x.totalAsientos, 2) 
                        : 0
                })
                .OrderByDescending(x => x.porcentajeOcupacion)
                .ToList();

            // 3. Detalle por Viaje (Proyección optimizada)
            var ocupacionPorViaje = await query
                .Select(v => new
                {
                    v.ViajeID,
                    v.CodigoViaje,
                    eventoNombre = v.Evento.Nombre,
                    rutaNombre = v.PlantillaRuta.NombreRuta,
                    v.FechaSalida,
                    v.CupoTotal,
                    v.AsientosVendidos,
                    v.AsientosDisponibles,
                    unidadPlacas = v.Unidad != null ? v.Unidad.Placas : null
                })
                .ToListAsync();

            // Calcular porcentaje por viaje en memoria
            var ocupacionPorViajeFinal = ocupacionPorViaje
                .Select(v => new 
                {
                    v.ViajeID,
                    v.CodigoViaje,
                    v.eventoNombre,
                    v.rutaNombre,
                    v.FechaSalida,
                    v.CupoTotal,
                    v.AsientosVendidos,
                    v.AsientosDisponibles,
                    v.unidadPlacas,
                    porcentajeOcupacion = v.CupoTotal > 0 
                        ? Math.Round((v.AsientosVendidos * 100.0) / v.CupoTotal, 2) 
                        : 0
                })
                .OrderByDescending(x => x.porcentajeOcupacion)
                .ToList();

            var reporte = new
            {
                periodo = new { desde, hasta },
                totalViajes = stats?.TotalViajes ?? 0,
                viajesCompletos = stats?.ViajesCompletos ?? 0,
                promedioOcupacion = (stats?.TotalAsientos ?? 0) > 0 
                    ? Math.Round(((stats?.TotalVendidos ?? 0) * 100.0) / (stats?.TotalAsientos ?? 1), 2) 
                    : 0,
                totalAsientosDisponibles = stats?.TotalAsientos ?? 0,
                totalAsientosVendidos = stats?.TotalVendidos ?? 0,
                ocupacionPorViaje = ocupacionPorViajeFinal,
                ocupacionPorEvento
            };

            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ocupación");
            return StatusCode(500, new { message = "Error al generar reporte", error = ex.Message });
        }
    }

    /// <summary>
    /// Dashboard general con métricas principales
    /// GET /api/reportes/dashboard
    /// </summary>
    [HttpGet("dashboard")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public async Task<ActionResult> GetDashboard()
    {
        try
        {
            var hoy = DateTime.Now.Date;
            var mesActual = new DateTime(hoy.Year, hoy.Month, 1);

            // Métricas de boletos
            var boletosHoy = await _context.Boletos.CountAsync(b => b.FechaCompra.Date == hoy);
            var boletosMes = await _context.Boletos.CountAsync(b => b.FechaCompra >= mesActual);
            var ingresosMes = await _context.Boletos
                .Where(b => b.FechaCompra >= mesActual && (b.Estatus == 10 || b.Estatus == 11))
                .SumAsync(b => b.PrecioTotal);

            // Métricas de viajes
            var viajesProximos = await _context.Viajes
                .CountAsync(v => v.FechaSalida >= hoy && v.FechaSalida <= hoy.AddDays(7));
            var viajesHoy = await _context.Viajes
                .CountAsync(v => v.FechaSalida.Date == hoy);

            // Métricas de usuarios
            var usuariosActivos = await _context.Users.CountAsync(u => u.Estatus == 1);
            var usuariosNuevosMes = await _context.Users
                .CountAsync(u => u.FechaRegistro >= mesActual);

            // Incidencias abiertas (Estatus: 1=Activo, 2=EnProceso)
            var incidenciasAbiertas = await _context.Incidencias
                .CountAsync(i => i.Estatus == 1 || i.Estatus == 2);

            // Eventos activos
            var eventosActivos = await _context.Eventos
                .CountAsync(e => e.Estatus == 1);

            var dashboard = new
            {
                metricas = new
                {
                    boletosHoy,
                    boletosMes,
                    ingresosMes,
                    viajesProximos,
                    viajesHoy,
                    usuariosActivos,
                    usuariosNuevosMes,
                    incidenciasAbiertas,
                    eventosActivos
                },
                ultimosEventos = await _context.Eventos
                    .OrderByDescending(e => e.FechaCreacion)
                    .Take(5)
                    .Select(e => new
                    {
                        e.EventoID,
                        e.Nombre,
                        e.Fecha,
                        e.Ciudad,
                        totalViajes = e.Viajes.Count
                    })
                    .ToListAsync(),
                proximosViajes = await _context.Viajes
                    .Include(v => v.Evento)
                    .Include(v => v.PlantillaRuta)
                    .Where(v => v.FechaSalida >= hoy)
                    .OrderBy(v => v.FechaSalida)
                    .Take(10)
                    .Select(v => new
                    {
                        v.ViajeID,
                        v.CodigoViaje,
                        eventoNombre = v.Evento.Nombre,
                        rutaNombre = v.PlantillaRuta.NombreRuta,
                        v.FechaSalida,
                        v.AsientosVendidos,
                        v.CupoTotal,
                        ocupacion = Math.Round((v.AsientosVendidos * 100.0) / v.CupoTotal, 2)
                    })
                    .ToListAsync()
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar dashboard");
            return StatusCode(500, new { message = "Error al generar dashboard", error = ex.Message });
        }
    }
}
