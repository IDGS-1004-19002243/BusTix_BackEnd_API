using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Security;

namespace prjBusTix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class ReportesController : ControllerBase
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

            // Constantes para estatus de boletos
            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;
            const int ESTATUS_BOLETO_CANCELADO = 12;

            // 1. Totales Generales (Ejecutado en BD) - Sin GroupBy para manejar resultados vacíos
            var totalBoletos = await query.CountAsync();
            var boletosVendidos = await query.CountAsync(b => b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO);
            var boletosCancelados = await query.CountAsync(b => b.Estatus == ESTATUS_BOLETO_CANCELADO);
            
            var boletosValidosQuery = query.Where(b => b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO);
            var ingresoTotal = await boletosValidosQuery.SumAsync(b => (decimal?)b.PrecioTotal) ?? 0;
            var ingresoBase = await boletosValidosQuery.SumAsync(b => (decimal?)b.PrecioBase) ?? 0;
            var descuentos = await boletosValidosQuery.SumAsync(b => (decimal?)b.Descuento) ?? 0;
            var cargos = await boletosValidosQuery.SumAsync(b => (decimal?)b.CargoServicio) ?? 0;

            // 2. Ventas por Evento (Ejecutado en BD)
            var ventasPorEvento = await query
                .GroupBy(b => new { b.Viaje.EventoID, b.Viaje.Evento.Nombre })
                .Select(g => new
                {
                    eventoId = g.Key.EventoID,
                    eventoNombre = g.Key.Nombre ?? "Sin evento",
                    totalBoletos = g.Count(),
                    boletosVendidos = g.Count(b => b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO),
                    ingresoTotal = g.Where(b => b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO)
                                    .Sum(b => (decimal?)b.PrecioTotal) ?? 0
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
                    boletosVendidos = g.Count(b => b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO),
                    ingresoTotal = g.Where(b => b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO)
                                    .Sum(b => (decimal?)b.PrecioTotal) ?? 0
                })
                .OrderBy(x => x.fecha)
                .ToListAsync();

            var reporte = new
            {
                periodo = new { desde, hasta },
                totalBoletos,
                boletosVendidos,
                boletosCancelados,
                ingresoTotal,
                ingresoBase,
                descuentosAplicados = descuentos,
                cargosServicio = cargos,
                iva = ingresoTotal - ingresoBase - cargos + descuentos, // Calcular IVA
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

            // 1. Totales Generales (BD) - Sin GroupBy para manejar vacíos
            var totalViajes = await query.CountAsync();
            var viajesCompletos = await query.CountAsync(v => v.AsientosDisponibles == 0);
            var totalAsientos = await query.SumAsync(v => (int?)v.CupoTotal) ?? 0;
            var totalVendidos = await query.SumAsync(v => (int?)v.AsientosVendidos) ?? 0;

            // 2. Ocupación por Evento (BD)
            var rawOcupacionEvento = await query
                .GroupBy(v => new { v.EventoID, v.Evento.Nombre })
                .Select(g => new
                {
                    eventoId = g.Key.EventoID,
                    eventoNombre = g.Key.Nombre ?? "Sin evento",
                    totalViajes = g.Count(),
                    totalAsientos = g.Sum(v => (int?)v.CupoTotal) ?? 0,
                    asientosVendidos = g.Sum(v => (int?)v.AsientosVendidos) ?? 0
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
                    eventoNombre = v.Evento != null ? v.Evento.Nombre : "Sin evento",
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
                totalViajes,
                viajesCompletos,
                promedioOcupacion = totalAsientos > 0 
                    ? Math.Round((totalVendidos * 100.0) / totalAsientos, 2) 
                    : 0,
                totalAsientosDisponibles = totalAsientos,
                totalAsientosVendidos = totalVendidos,
                asientosLibres = totalAsientos - totalVendidos,
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

            // Constantes para estatus
            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;
            const int ESTATUS_ACTIVO = 1;
            const int ESTATUS_EN_PROCESO = 2;

            // Métricas de boletos
            var boletosHoy = await _context.Boletos.CountAsync(b => b.FechaCompra.Date == hoy);
            var boletosMes = await _context.Boletos.CountAsync(b => b.FechaCompra >= mesActual);
            var ingresosMes = await _context.Boletos
                .Where(b => b.FechaCompra >= mesActual && (b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO))
                .SumAsync(b => (decimal?)b.PrecioTotal) ?? 0;

            // Métricas de viajes
            var viajesProximos = await _context.Viajes
                .CountAsync(v => v.FechaSalida >= hoy && v.FechaSalida <= hoy.AddDays(7));
            var viajesHoy = await _context.Viajes
                .CountAsync(v => v.FechaSalida.Date == hoy);

            // Métricas de usuarios
            var usuariosActivos = await _context.Users.CountAsync(u => u.Estatus == ESTATUS_ACTIVO);
            var usuariosNuevosMes = await _context.Users
                .CountAsync(u => u.FechaRegistro >= mesActual);

            // Incidencias abiertas (Estatus: 1=Activo, 2=EnProceso)
            var incidenciasAbiertas = await _context.Incidencias
                .CountAsync(i => i.Estatus == ESTATUS_ACTIVO || i.Estatus == ESTATUS_EN_PROCESO);

            // Eventos activos
            var eventosActivos = await _context.Eventos
                .CountAsync(e => e.Estatus == ESTATUS_ACTIVO);

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
                        eventoNombre = v.Evento != null ? v.Evento.Nombre : "Sin evento",
                        rutaNombre = v.PlantillaRuta.NombreRuta,
                        v.FechaSalida,
                        v.AsientosVendidos,
                        v.CupoTotal,
                        v.AsientosDisponibles,
                        porcentajeOcupacion = v.CupoTotal > 0 
                            ? Math.Round((v.AsientosVendidos * 100.0) / v.CupoTotal, 2)
                            : 0
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
