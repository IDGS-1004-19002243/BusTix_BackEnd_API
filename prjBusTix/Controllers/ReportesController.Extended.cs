using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Security;

namespace prjBusTix.Controllers;

public partial class ReportesController
{
    /// <summary>
    /// Reporte de desempeño por ruta
    /// GET /api/reportes/rutas
    /// </summary>
    [HttpGet("rutas")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public async Task<ActionResult> GetReporteRutas(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;

            var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
            var hasta = fechaHasta ?? DateTime.Now;

            var rutasStats = await _context.Viajes
                .Where(v => v.FechaCreacion >= desde && v.FechaCreacion <= hasta)
                .GroupBy(v => new { v.PlantillaRutaID, v.PlantillaRuta.NombreRuta, v.PlantillaRuta.CiudadOrigen, v.PlantillaRuta.CiudadDestino })
                .Select(g => new
                {
                    rutaId = g.Key.PlantillaRutaID,
                    nombreRuta = g.Key.NombreRuta,
                    ciudadOrigen = g.Key.CiudadOrigen,
                    ciudadDestino = g.Key.CiudadDestino,
                    totalViajes = g.Count(),
                    totalAsientos = g.Sum(v => v.CupoTotal),
                    asientosVendidos = g.Sum(v => v.AsientosVendidos),
                    ocupacionPromedio = g.Average(v => v.CupoTotal > 0 ? (v.AsientosVendidos * 100.0) / v.CupoTotal : 0)
                })
                .ToListAsync();

            var ingresosRutas = await _context.Boletos
                .Where(b => b.FechaCompra >= desde && b.FechaCompra <= hasta &&
                           (b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO))
                .GroupBy(b => b.Viaje.PlantillaRutaID)
                .Select(g => new
                {
                    rutaId = g.Key,
                    ingresoTotal = g.Sum(b => b.PrecioTotal),
                    totalBoletos = g.Count()
                })
                .ToListAsync();

            var resultado = rutasStats.Select(r =>
            {
                var ingresos = ingresosRutas.FirstOrDefault(i => i.rutaId == r.rutaId);
                return new
                {
                    r.rutaId,
                    r.nombreRuta,
                    r.ciudadOrigen,
                    r.ciudadDestino,
                    r.totalViajes,
                    r.totalAsientos,
                    r.asientosVendidos,
                    ocupacionPromedio = Math.Round(r.ocupacionPromedio, 2),
                    ingresoTotal = ingresos?.ingresoTotal ?? 0,
                    totalBoletos = ingresos?.totalBoletos ?? 0,
                    ingresoPromedioPorViaje = r.totalViajes > 0 ? Math.Round((ingresos?.ingresoTotal ?? 0) / r.totalViajes, 2) : 0
                };
            })
            .OrderByDescending(r => r.ingresoTotal)
            .ToList();

            return Ok(new
            {
                success = true,
                desde,
                hasta,
                totalRutas = resultado.Count,
                rutas = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de rutas");
            return StatusCode(500, new { message = "Error al generar reporte de rutas", error = ex.Message });
        }
    }

    /// <summary>
    /// Reporte de puntualidad de viajes
    /// GET /api/reportes/puntualidad
    /// </summary>
    [HttpGet("puntualidad")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public async Task<ActionResult> GetReportePuntualidad(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            const int ESTATUS_ACTIVO = 1;
            const int ESTATUS_EN_PROCESO = 2;
            const int ESTATUS_COMPLETADO = 3;

            var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
            var hasta = fechaHasta ?? DateTime.Now;

            var viajes = await _context.Viajes
                .Include(v => v.Chofer)
                .Include(v => v.Evento)
                .Where(v => v.FechaSalida >= desde && v.FechaSalida <= hasta)
                .Select(v => new
                {
                    v.ViajeID,
                    v.CodigoViaje,
                    v.FechaSalida,
                    v.FechaLlegadaEstimada,
                    v.Estatus,
                    eventoNombre = v.Evento.Nombre,
                    choferNombre = v.Chofer != null ? v.Chofer.NombreCompleto : "Sin asignar",
                    choferId = v.ChoferID
                })
                .ToListAsync();

            var totalViajes = viajes.Count;
            var viajesCompletados = viajes.Count(v => v.Estatus == ESTATUS_COMPLETADO);
            var viajesEnProceso = viajes.Count(v => v.Estatus == ESTATUS_EN_PROCESO);
            var viajesProgramados = viajes.Count(v => v.Estatus == ESTATUS_ACTIVO);

            // Agrupar por chofer
            var porChofer = viajes
                .Where(v => !string.IsNullOrEmpty(v.choferId))
                .GroupBy(v => new { v.choferId, v.choferNombre })
                .Select(g => new
                {
                    choferId = g.Key.choferId,
                    choferNombre = g.Key.choferNombre,
                    totalViajes = g.Count(),
                    completados = g.Count(v => v.Estatus == ESTATUS_COMPLETADO),
                    enProceso = g.Count(v => v.Estatus == ESTATUS_EN_PROCESO),
                    programados = g.Count(v => v.Estatus == ESTATUS_ACTIVO)
                })
                .OrderByDescending(c => c.totalViajes)
                .ToList();

            return Ok(new
            {
                success = true,
                desde,
                hasta,
                resumen = new
                {
                    totalViajes,
                    viajesCompletados,
                    viajesEnProceso,
                    viajesProgramados,
                    porcentajeCompletados = totalViajes > 0 ? Math.Round((viajesCompletados * 100.0) / totalViajes, 2) : 0
                },
                desempeñoChoferes = porChofer
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de puntualidad");
            return StatusCode(500, new { message = "Error al generar reporte de puntualidad", error = ex.Message });
        }
    }

    /// <summary>
    /// Reporte de uso y desempeño de unidades
    /// GET /api/reportes/unidades
    /// </summary>
    [HttpGet("unidades")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public async Task<ActionResult> GetReporteUnidades(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;

            var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
            var hasta = fechaHasta ?? DateTime.Now;

            var unidadesStats = await _context.Viajes
                .Where(v => v.UnidadID != null && v.FechaCreacion >= desde && v.FechaCreacion <= hasta)
                .GroupBy(v => new { v.UnidadID, v.Unidad!.Placas, v.Unidad.Modelo, v.Unidad.CapacidadAsientos })
                .Select(g => new
                {
                    unidadId = g.Key.UnidadID,
                    placas = g.Key.Placas,
                    modelo = g.Key.Modelo,
                    capacidad = g.Key.CapacidadAsientos,
                    totalViajes = g.Count(),
                    totalAsientos = g.Sum(v => v.CupoTotal),
                    asientosVendidos = g.Sum(v => v.AsientosVendidos),
                    ocupacionPromedio = g.Average(v => v.CupoTotal > 0 ? (v.AsientosVendidos * 100.0) / v.CupoTotal : 0)
                })
                .ToListAsync();

            var ingresosUnidades = await _context.Boletos
                .Where(b => b.Viaje.UnidadID != null && 
                           b.FechaCompra >= desde && b.FechaCompra <= hasta &&
                           (b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO))
                .GroupBy(b => b.Viaje.UnidadID)
                .Select(g => new
                {
                    unidadId = g.Key,
                    ingresoTotal = g.Sum(b => b.PrecioTotal)
                })
                .ToListAsync();

            var incidenciasUnidades = await _context.Incidencias
                .Where(i => i.UnidadID != null && i.FechaReporte >= desde && i.FechaReporte <= hasta)
                .GroupBy(i => i.UnidadID)
                .Select(g => new
                {
                    unidadId = g.Key,
                    totalIncidencias = g.Count(),
                    incidenciasCriticas = g.Count(i => i.Prioridad == "Crítica")
                })
                .ToListAsync();

            var resultado = unidadesStats.Select(u =>
            {
                var ingresos = ingresosUnidades.FirstOrDefault(i => i.unidadId == u.unidadId);
                var incidencias = incidenciasUnidades.FirstOrDefault(i => i.unidadId == u.unidadId);
                return new
                {
                    u.unidadId,
                    u.placas,
                    u.modelo,
                    u.capacidad,
                    u.totalViajes,
                    ocupacionPromedio = Math.Round(u.ocupacionPromedio, 2),
                    ingresoTotal = ingresos?.ingresoTotal ?? 0,
                    ingresoPorViaje = u.totalViajes > 0 ? Math.Round((ingresos?.ingresoTotal ?? 0) / u.totalViajes, 2) : 0,
                    totalIncidencias = incidencias?.totalIncidencias ?? 0,
                    incidenciasCriticas = incidencias?.incidenciasCriticas ?? 0,
                    eficiencia = new
                    {
                        viajesPorDia = Math.Round(u.totalViajes / (decimal)((hasta - desde).TotalDays > 0 ? (hasta - desde).TotalDays : 1), 2),
                        ingresoPorDia = Math.Round((ingresos?.ingresoTotal ?? 0) / (decimal)((hasta - desde).TotalDays > 0 ? (hasta - desde).TotalDays : 1), 2)
                    }
                };
            })
            .OrderByDescending(u => u.ingresoTotal)
            .ToList();

            return Ok(new
            {
                success = true,
                desde,
                hasta,
                totalUnidades = resultado.Count,
                unidades = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de unidades");
            return StatusCode(500, new { message = "Error al generar reporte de unidades", error = ex.Message });
        }
    }

    /// <summary>
    /// Reporte de efectividad de cupones
    /// GET /api/reportes/cupones
    /// </summary>
    [HttpGet("cupones")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public ActionResult GetReporteCupones(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
            var hasta = fechaHasta ?? DateTime.Now;

            // Comentado: El modelo Boleto no tiene CuponID actualmente
            // Este reporte estará disponible cuando se implemente el sistema de cupones

            return Ok(new
            {
                success = true,
                desde,
                hasta,
                message = "Reporte de cupones no disponible. El sistema de cupones aún no está implementado en el modelo.",
                resumen = new
                {
                    totalCuponesActivos = 0,
                    totalUsos = 0,
                    descuentoTotalOtorgado = 0.0,
                    ingresosTotalesGenerados = 0.0,
                    roi = 0.0,
                    promedioDescuentoPorUso = 0.0
                },
                cupones = new List<object>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de cupones");
            return StatusCode(500, new { message = "Error al generar reporte de cupones", error = ex.Message });
        }
    }

    /// <summary>
    /// Reporte detallado de incidencias
    /// GET /api/reportes/incidencias-detalle
    /// </summary>
    [HttpGet("incidencias-detalle")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public async Task<ActionResult> GetReporteIncidenciasDetalle(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
            var hasta = fechaHasta ?? DateTime.Now;

            var incidencias = await _context.Incidencias
                .Include(i => i.TipoIncidencia)
                .Include(i => i.Unidad)
                .Include(i => i.EstatusNavigation)
                .Where(i => i.FechaReporte >= desde && i.FechaReporte <= hasta)
                .ToListAsync();

            var porTipo = incidencias
                .GroupBy(i => new { i.TipoIncidenciaID, i.TipoIncidencia.Nombre, i.TipoIncidencia.Categoria })
                .Select(g => new
                {
                    tipoId = g.Key.TipoIncidenciaID,
                    nombre = g.Key.Nombre,
                    categoria = g.Key.Categoria ?? "General",
                    total = g.Count(),
                    abiertas = g.Count(i => i.Estatus == 1),
                    resueltas = g.Count(i => i.Estatus == 3 || i.Estatus == 4),
                    tiempoPromedioResolucionHoras = g.Where(i => i.FechaResolucion.HasValue)
                        .Average(i => (i.FechaResolucion!.Value - i.FechaReporte).TotalHours)
                })
                .OrderByDescending(t => t.total)
                .ToList();

            var porPrioridad = incidencias
                .GroupBy(i => i.Prioridad)
                .Select(g => new
                {
                    prioridad = g.Key,
                    total = g.Count(),
                    porcentaje = Math.Round((g.Count() * 100.0) / incidencias.Count, 2)
                })
                .ToList();

            var unidadesConMasIncidencias = incidencias
                .Where(i => i.UnidadID != null)
                .GroupBy(i => new { i.UnidadID, i.Unidad!.Placas })
                .Select(g => new
                {
                    unidadId = g.Key.UnidadID,
                    placas = g.Key.Placas,
                    totalIncidencias = g.Count(),
                    criticas = g.Count(i => i.Prioridad == "Crítica")
                })
                .OrderByDescending(u => u.totalIncidencias)
                .Take(10)
                .ToList();

            var totalResueltas = incidencias.Count(i => i.FechaResolucion.HasValue);
            var tiempoPromedioGlobal = incidencias
                .Where(i => i.FechaResolucion.HasValue)
                .Average(i => (i.FechaResolucion!.Value - i.FechaReporte).TotalHours);

            return Ok(new
            {
                success = true,
                desde,
                hasta,
                resumen = new
                {
                    totalIncidencias = incidencias.Count,
                    abiertas = incidencias.Count(i => i.Estatus == 1),
                    enProceso = incidencias.Count(i => i.Estatus == 2),
                    resueltas = totalResueltas,
                    cerradas = incidencias.Count(i => i.Estatus == 4),
                    tiempoPromedioResolucionHoras = totalResueltas > 0 ? Math.Round(tiempoPromedioGlobal, 2) : 0,
                    tasaResolucion = incidencias.Count > 0 ? Math.Round((totalResueltas * 100.0) / incidencias.Count, 2) : 0
                },
                porTipo,
                porPrioridad,
                unidadesConMasIncidencias
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de incidencias");
            return StatusCode(500, new { message = "Error al generar reporte de incidencias", error = ex.Message });
        }
    }

    /// <summary>
    /// Reporte de comparación entre períodos
    /// GET /api/reportes/comparacion
    /// </summary>
    [HttpGet("comparacion")]
    [ClRequirePermission(ClAppPermissions.ReportesView)]
    public async Task<ActionResult> GetReporteComparacion(
        [FromQuery] DateTime? periodo1Desde,
        [FromQuery] DateTime? periodo1Hasta,
        [FromQuery] DateTime? periodo2Desde,
        [FromQuery] DateTime? periodo2Hasta)
    {
        try
        {
            const int ESTATUS_BOLETO_PAGADO = 10;
            const int ESTATUS_BOLETO_USADO = 11;

            // Período 1 (actual por defecto: mes actual)
            var p1Desde = periodo1Desde ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var p1Hasta = periodo1Hasta ?? DateTime.Now;

            // Período 2 (anterior por defecto: mes anterior)
            var p2Desde = periodo2Desde ?? p1Desde.AddMonths(-1);
            var p2Hasta = periodo2Hasta ?? p1Desde.AddDays(-1);

            // Ventas período 1
            var ventas1 = await _context.Boletos
                .Where(b => b.FechaCompra >= p1Desde && b.FechaCompra <= p1Hasta &&
                           (b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO))
                .Select(b => new { b.PrecioTotal, b.BoletoID })
                .ToListAsync();

            // Ventas período 2
            var ventas2 = await _context.Boletos
                .Where(b => b.FechaCompra >= p2Desde && b.FechaCompra <= p2Hasta &&
                           (b.Estatus == ESTATUS_BOLETO_PAGADO || b.Estatus == ESTATUS_BOLETO_USADO))
                .Select(b => new { b.PrecioTotal, b.BoletoID })
                .ToListAsync();

            var ingresos1 = ventas1.Sum(b => b.PrecioTotal);
            var ingresos2 = ventas2.Sum(b => b.PrecioTotal);
            var boletos1 = ventas1.Count;
            var boletos2 = ventas2.Count;

            var crecimientoIngresos = ingresos2 > 0 ? Math.Round(((ingresos1 - ingresos2) / ingresos2) * 100, 2) : 0;
            var crecimientoBoletos = boletos2 > 0 ? Math.Round(((boletos1 - boletos2) / (double)boletos2) * 100, 2) : 0;

            // Viajes período 1 y 2
            var viajes1 = await _context.Viajes
                .CountAsync(v => v.FechaCreacion >= p1Desde && v.FechaCreacion <= p1Hasta);

            var viajes2 = await _context.Viajes
                .CountAsync(v => v.FechaCreacion >= p2Desde && v.FechaCreacion <= p2Hasta);

            var crecimientoViajes = viajes2 > 0 ? Math.Round(((viajes1 - viajes2) / (double)viajes2) * 100, 2) : 0;

            return Ok(new
            {
                success = true,
                periodo1 = new { desde = p1Desde, hasta = p1Hasta },
                periodo2 = new { desde = p2Desde, hasta = p2Hasta },
                comparacion = new
                {
                    ingresos = new
                    {
                        periodo1 = Math.Round(ingresos1, 2),
                        periodo2 = Math.Round(ingresos2, 2),
                        diferencia = Math.Round(ingresos1 - ingresos2, 2),
                        crecimientoPorcentaje = crecimientoIngresos,
                        tendencia = crecimientoIngresos >= 0 ? "positiva" : "negativa"
                    },
                    boletos = new
                    {
                        periodo1 = boletos1,
                        periodo2 = boletos2,
                        diferencia = boletos1 - boletos2,
                        crecimientoPorcentaje = crecimientoBoletos,
                        tendencia = crecimientoBoletos >= 0 ? "positiva" : "negativa"
                    },
                    viajes = new
                    {
                        periodo1 = viajes1,
                        periodo2 = viajes2,
                        diferencia = viajes1 - viajes2,
                        crecimientoPorcentaje = crecimientoViajes,
                        tendencia = crecimientoViajes >= 0 ? "positiva" : "negativa"
                    },
                    ticketPromedio = new
                    {
                        periodo1 = boletos1 > 0 ? Math.Round(ingresos1 / boletos1, 2) : 0,
                        periodo2 = boletos2 > 0 ? Math.Round(ingresos2 / boletos2, 2) : 0
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de comparación");
            return StatusCode(500, new { message = "Error al generar reporte de comparación", error = ex.Message });
        }
    }
}
