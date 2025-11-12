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
    /// Reporte de ventas por período
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

            var query = _context.Boletos
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.Evento)
                .Include(b => b.Viaje)
                    .ThenInclude(v => v.PlantillaRuta)
                .Where(b => b.FechaCompra >= desde && b.FechaCompra <= hasta);

            if (eventoId.HasValue)
                query = query.Where(b => b.Viaje.EventoID == eventoId.Value);

            var boletos = await query.ToListAsync();

            var reporte = new
            {
                periodo = new { desde, hasta },
                totalBoletos = boletos.Count,
                boletosVendidos = boletos.Count(b => b.Estatus == 10 || b.Estatus == 11),
                boletosCancelados = boletos.Count(b => b.Estatus == 12),
                ingresoTotal = boletos.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => b.PrecioTotal),
                ingresoBase = boletos.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => b.PrecioBase),
                descuentosAplicados = boletos.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => b.Descuento),
                cargosServicio = boletos.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => b.CargoServicio),
                ventasPorEvento = boletos
                    .GroupBy(b => new { b.Viaje.EventoID, b.Viaje.Evento.Nombre })
                    .Select(g => new
                    {
                        eventoId = g.Key.EventoID,
                        eventoNombre = g.Key.Nombre,
                        totalBoletos = g.Count(),
                        ingresoTotal = g.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => b.PrecioTotal)
                    })
                    .OrderByDescending(x => x.ingresoTotal)
                    .ToList(),
                ventasPorDia = boletos
                    .GroupBy(b => b.FechaCompra.Date)
                    .Select(g => new
                    {
                        fecha = g.Key,
                        totalBoletos = g.Count(),
                        ingresoTotal = g.Where(b => b.Estatus == 10 || b.Estatus == 11).Sum(b => b.PrecioTotal)
                    })
                    .OrderBy(x => x.fecha)
                    .ToList()
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
    /// Reporte de ocupación de viajes
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

            var query = _context.Viajes
                .Include(v => v.Evento)
                .Include(v => v.PlantillaRuta)
                .Include(v => v.Unidad)
                .Where(v => v.FechaSalida >= desde && v.FechaSalida <= hasta);

            if (eventoId.HasValue)
                query = query.Where(v => v.EventoID == eventoId.Value);

            var viajes = await query.ToListAsync();

            var reporte = new
            {
                periodo = new { desde, hasta },
                totalViajes = viajes.Count,
                viajesCompletos = viajes.Count(v => v.AsientosDisponibles == 0),
                promedioOcupacion = viajes.Any() 
                    ? Math.Round(viajes.Average(v => (v.AsientosVendidos * 100.0) / v.CupoTotal), 2)
                    : 0,
                totalAsientosDisponibles = viajes.Sum(v => v.CupoTotal),
                totalAsientosVendidos = viajes.Sum(v => v.AsientosVendidos),
                ocupacionPorViaje = viajes
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
                        porcentajeOcupacion = Math.Round((v.AsientosVendidos * 100.0) / v.CupoTotal, 2),
                        unidadPlacas = v.Unidad != null ? v.Unidad.Placas : null
                    })
                    .OrderByDescending(x => x.porcentajeOcupacion)
                    .ToList(),
                ocupacionPorEvento = viajes
                    .GroupBy(v => new { v.EventoID, v.Evento.Nombre })
                    .Select(g => new
                    {
                        eventoId = g.Key.EventoID,
                        eventoNombre = g.Key.Nombre,
                        totalViajes = g.Count(),
                        totalAsientos = g.Sum(v => v.CupoTotal),
                        asientosVendidos = g.Sum(v => v.AsientosVendidos),
                        porcentajeOcupacion = Math.Round((g.Sum(v => v.AsientosVendidos) * 100.0) / g.Sum(v => v.CupoTotal), 2)
                    })
                    .OrderByDescending(x => x.porcentajeOcupacion)
                    .ToList()
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
