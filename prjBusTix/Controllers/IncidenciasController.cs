using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Dto.Incidencias;
using prjBusTix.Model;
using prjBusTix.Security;
using System.Security.Claims;

namespace prjBusTix.Controllers;

/// <summary>
/// Controlador para la gestión completa de incidencias del sistema BusTix
/// Permite al staff reportar incidencias y a los administradores gestionarlas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncidenciasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ClApplicationUser> _userManager;
    private readonly ILogger<IncidenciasController> _logger;

    public IncidenciasController(
        AppDbContext context,
        UserManager<ClApplicationUser> userManager,
        ILogger<IncidenciasController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    #region Endpoints para Administradores (PWA)

    /// <summary>
    /// Obtener todas las incidencias con filtros y paginación
    /// GET: api/incidencias
    /// </summary>
    /// <remarks>
    /// Este endpoint es usado por los administradores en la PWA para ver el dashboard de incidencias.
    /// Soporta múltiples filtros y paginación.
    /// </remarks>
    [HttpGet]
    [ClRequirePermission(AppPermissions.Incidencias.View)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetIncidencias([FromQuery] FiltroIncidenciasDto filtro)
    {
        try
        {
            var query = _context.Incidencias
                .Include(i => i.TipoIncidencia)
                .Include(i => i.Reportador)
                .Include(i => i.Asignado)
                .Include(i => i.EstatusNavigation)
                .Include(i => i.Viaje)
                .Include(i => i.Unidad)
                .AsQueryable();

            // Aplicar filtros
            if (filtro.Estatus.HasValue)
            {
                query = query.Where(i => i.Estatus == filtro.Estatus.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Prioridad))
            {
                query = query.Where(i => i.Prioridad == filtro.Prioridad);
            }

            if (filtro.ViajeID.HasValue)
            {
                query = query.Where(i => i.ViajeID == filtro.ViajeID.Value);
            }

            if (filtro.TipoIncidenciaID.HasValue)
            {
                query = query.Where(i => i.TipoIncidenciaID == filtro.TipoIncidenciaID.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtro.AsignadoA))
            {
                query = query.Where(i => i.AsignadoA == filtro.AsignadoA);
            }

            if (!string.IsNullOrWhiteSpace(filtro.ReportadoPor))
            {
                query = query.Where(i => i.ReportadoPor == filtro.ReportadoPor);
            }

            if (filtro.FechaDesde.HasValue)
            {
                query = query.Where(i => i.FechaReporte >= filtro.FechaDesde.Value);
            }

            if (filtro.FechaHasta.HasValue)
            {
                var fechaHastaFinal = filtro.FechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(i => i.FechaReporte <= fechaHastaFinal);
            }

            // Búsqueda por texto en título o descripción
            if (!string.IsNullOrWhiteSpace(filtro.TextoBusqueda))
            {
                var busqueda = filtro.TextoBusqueda.ToLower();
                query = query.Where(i => 
                    i.Titulo.ToLower().Contains(busqueda) || 
                    i.Descripcion.ToLower().Contains(busqueda) ||
                    i.CodigoIncidencia.ToLower().Contains(busqueda));
            }

            // Aplicar ordenamiento
            query = AplicarOrdenamiento(query, filtro.OrdenarPor, filtro.DireccionOrden);

            // Obtener el total antes de paginar
            var total = await query.CountAsync();

            // Validar paginación
            if (filtro.Pagina < 1) filtro.Pagina = 1;
            if (filtro.TamanoPagina < 1) filtro.TamanoPagina = 20;
            if (filtro.TamanoPagina > 100) filtro.TamanoPagina = 100;

            // Aplicar paginación y proyección
            var incidencias = await query
                .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
                .Take(filtro.TamanoPagina)
                .Select(i => MapearAIncidenciaResponse(i))
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = incidencias,
                totalRegistros = total,
                pagina = filtro.Pagina,
                tamanoPagina = filtro.TamanoPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)filtro.TamanoPagina)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las incidencias con filtros: {@Filtro}", filtro);
            return StatusCode(500, new
            {
                success = false,
                message = "Error al obtener las incidencias",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtener una incidencia específica por ID
    /// GET: api/incidencias/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ClRequirePermission(AppPermissions.Incidencias.View)]
    [ProducesResponseType(typeof(IncidenciaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetIncidencia(int id)
    {
        try
        {
            var incidencia = await _context.Incidencias
                .Include(i => i.TipoIncidencia)
                .Include(i => i.Reportador)
                .Include(i => i.Asignado)
                .Include(i => i.EstatusNavigation)
                .Include(i => i.Viaje)
                .Include(i => i.Unidad)
                .FirstOrDefaultAsync(i => i.IncidenciaID == id);

            if (incidencia == null)
            {
                _logger.LogWarning("Incidencia con ID {IncidenciaId} no encontrada", id);
                return NotFound(new
                {
                    success = false,
                    message = $"Incidencia con ID {id} no encontrada"
                });
            }

            var response = MapearAIncidenciaResponse(incidencia);

            return Ok(new
            {
                success = true,
                data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la incidencia {IncidenciaId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error al obtener la incidencia",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Actualizar el estado, prioridad o asignación de una incidencia
    /// PUT: api/incidencias/{id}
    /// </summary>
    /// <remarks>
    /// Este endpoint es usado por administradores para gestionar incidencias:
    /// - Cambiar el estatus (Abierta → En Proceso → Resuelta → Cerrada)
    /// - Modificar la prioridad
    /// - Asignar a un técnico o responsable
    /// </remarks>
    [HttpPut("{id}")]
    [ClRequirePermission(AppPermissions.Incidencias.Update)]
    [ProducesResponseType(typeof(IncidenciaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ActualizarIncidencia(int id, [FromBody] ActualizarIncidenciaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datos de actualización inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var incidencia = await _context.Incidencias
                .Include(i => i.TipoIncidencia)
                .Include(i => i.Reportador)
                .Include(i => i.EstatusNavigation)
                .Include(i => i.Viaje)
                .Include(i => i.Unidad)
                .FirstOrDefaultAsync(i => i.IncidenciaID == id);

            if (incidencia == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Incidencia con ID {id} no encontrada"
                });
            }

            // Validar que el estatus existe
            var estatusExiste = await _context.EstatusGenerales
                .AnyAsync(e => e.Id_Estatus == dto.Estatus);

            if (!estatusExiste)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"El estatus {dto.Estatus} no existe en el sistema"
                });
            }

            // Validar usuario asignado si se proporciona
            if (!string.IsNullOrWhiteSpace(dto.AsignadoA))
            {
                var usuarioExiste = await _userManager.FindByIdAsync(dto.AsignadoA);
                if (usuarioExiste == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El usuario asignado no existe"
                    });
                }
            }

            // Guardar valores anteriores para log
            var estatusAnterior = incidencia.Estatus;
            var prioridadAnterior = incidencia.Prioridad;

            // Actualizar campos
            incidencia.Estatus = dto.Estatus;

            if (!string.IsNullOrWhiteSpace(dto.Prioridad))
            {
                incidencia.Prioridad = dto.Prioridad;
            }

            if (dto.AsignadoA != null)
            {
                incidencia.AsignadoA = string.IsNullOrWhiteSpace(dto.AsignadoA) ? null : dto.AsignadoA;
            }

            // Si el estatus cambia a Resuelta (3) o Cerrada (4), registrar fecha de resolución
            if ((dto.Estatus == 3 || dto.Estatus == 4) && 
                estatusAnterior != dto.Estatus && 
                !incidencia.FechaResolucion.HasValue)
            {
                incidencia.FechaResolucion = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Log de auditoría
            _logger.LogInformation(
                "Incidencia {IncidenciaId} actualizada. Estatus: {EstatusAnterior} → {EstatusNuevo}, Prioridad: {PrioridadAnterior} → {PrioridadNueva}",
                id, estatusAnterior, dto.Estatus, prioridadAnterior, incidencia.Prioridad);

            // Recargar con las relaciones actualizadas
            await _context.Entry(incidencia)
                .Reference(i => i.EstatusNavigation)
                .LoadAsync();

            if (!string.IsNullOrWhiteSpace(incidencia.AsignadoA))
            {
                await _context.Entry(incidencia)
                    .Reference(i => i.Asignado)
                    .LoadAsync();
            }

            var response = MapearAIncidenciaResponse(incidencia);

            return Ok(new
            {
                success = true,
                message = "Incidencia actualizada exitosamente",
                data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la incidencia {IncidenciaId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Error al actualizar la incidencia",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtener estadísticas de incidencias para el dashboard
    /// GET: api/incidencias/estadisticas
    /// </summary>
    /// <remarks>
    /// Retorna estadísticas agregadas útiles para el dashboard de administradores:
    /// - Total de incidencias por estatus
    /// - Distribución por prioridad
    /// - Distribución por tipo
    /// - Tiempos promedio de resolución
    /// </remarks>
    [HttpGet("estadisticas")]
    [ClRequirePermission(AppPermissions.Incidencias.View)]
    [ProducesResponseType(typeof(EstadisticasIncidenciasDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetEstadisticas()
    {
        try
        {
            var ahora = DateTime.Now;
            var inicioHoy = ahora.Date;
            var inicioSemana = ahora.AddDays(-(int)ahora.DayOfWeek);
            var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);

            var totalIncidencias = await _context.Incidencias.CountAsync();
            var abiertas = await _context.Incidencias.CountAsync(i => i.Estatus == 1);
            var enProceso = await _context.Incidencias.CountAsync(i => i.Estatus == 2);
            var resueltas = await _context.Incidencias.CountAsync(i => i.Estatus == 3);
            var cerradas = await _context.Incidencias.CountAsync(i => i.Estatus == 4);
            var canceladas = await _context.Incidencias.CountAsync(i => i.Estatus == 5);

            // Incidencias activas (no resueltas, no cerradas, no canceladas)
            var activas = await _context.Incidencias
                .Where(i => i.Estatus != 3 && i.Estatus != 4 && i.Estatus != 5)
                .ToListAsync();

            var porPrioridad = activas
                .GroupBy(i => i.Prioridad)
                .Select(g => new ContadorPrioridad
                {
                    Prioridad = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = totalIncidencias > 0 ? (g.Count() * 100.0 / totalIncidencias) : 0
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            var porTipo = await _context.Incidencias
                .Include(i => i.TipoIncidencia)
                .Where(i => i.Estatus != 3 && i.Estatus != 4 && i.Estatus != 5)
                .GroupBy(i => new { i.TipoIncidenciaID, i.TipoIncidencia.Nombre, i.TipoIncidencia.Categoria })
                .Select(g => new ContadorTipo
                {
                    TipoID = g.Key.TipoIncidenciaID,
                    Tipo = g.Key.Nombre,
                    Categoria = g.Key.Categoria,
                    Cantidad = g.Count(),
                    Porcentaje = totalIncidencias > 0 ? (g.Count() * 100.0 / totalIncidencias) : 0
                })
                .OrderByDescending(x => x.Cantidad)
                .ToListAsync();

            // Calcular tiempo promedio de resolución
            var incidenciasResueltas = await _context.Incidencias
                .Where(i => i.FechaResolucion.HasValue)
                .Select(i => new { i.FechaReporte, i.FechaResolucion })
                .ToListAsync();

            double tiempoPromedioHoras = 0;
            if (incidenciasResueltas.Any())
            {
                var tiempos = incidenciasResueltas
                    .Select(i => (i.FechaResolucion!.Value - i.FechaReporte).TotalHours)
                    .ToList();
                tiempoPromedioHoras = tiempos.Average();
            }

            var reportadasHoy = await _context.Incidencias
                .CountAsync(i => i.FechaReporte >= inicioHoy);

            var reportadasEstaSemana = await _context.Incidencias
                .CountAsync(i => i.FechaReporte >= inicioSemana);

            var reportadasEsteMes = await _context.Incidencias
                .CountAsync(i => i.FechaReporte >= inicioMes);

            var estadisticas = new EstadisticasIncidenciasDto
            {
                TotalIncidencias = totalIncidencias,
                Abiertas = abiertas,
                EnProceso = enProceso,
                Resueltas = resueltas,
                Cerradas = cerradas,
                Canceladas = canceladas,
                PorPrioridad = porPrioridad,
                PorTipo = porTipo,
                TiempoPromedioResolucionHoras = Math.Round(tiempoPromedioHoras, 2),
                ReportadasHoy = reportadasHoy,
                ReportadasEstaSemana = reportadasEstaSemana,
                ReportadasEsteMes = reportadasEsteMes
            };

            return Ok(new
            {
                success = true,
                data = estadisticas
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de incidencias");
            return StatusCode(500, new
            {
                success = false,
                message = "Error al obtener las estadísticas",
                error = ex.Message
            });
        }
    }

    #endregion

    #region Endpoints para Staff (Móvil)

    /// <summary>
    /// Crear una nueva incidencia (usado por Staff desde la app móvil)
    /// POST: api/incidencias
    /// </summary>
    /// <remarks>
    /// Este endpoint permite al staff reportar incidencias durante sus viajes.
    /// La incidencia se crea con estatus "Abierta" por defecto.
    /// </remarks>
    [HttpPost]
    [ClRequirePermission(AppPermissions.Incidencias.Create)]
    [ProducesResponseType(typeof(IncidenciaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CrearIncidencia([FromBody] CrearIncidenciaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datos de la incidencia inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // Obtener el usuario actual
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Usuario no autenticado"
                });
            }

            // Validar que el tipo de incidencia existe y está activo
            var tipoIncidencia = await _context.TipoIncidencia
                .FirstOrDefaultAsync(t => t.TipoIncidenciaID == dto.TipoIncidenciaID && t.EsActivo);

            if (tipoIncidencia == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El tipo de incidencia no existe o no está activo"
                });
            }

            // Validar viaje si se proporciona
            if (dto.ViajeID.HasValue)
            {
                var viajeExiste = await _context.Viajes.AnyAsync(v => v.ViajeID == dto.ViajeID.Value);
                if (!viajeExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"El viaje con ID {dto.ViajeID} no existe"
                    });
                }
            }

            // Validar unidad si se proporciona
            if (dto.UnidadID.HasValue)
            {
                var unidadExiste = await _context.Unidades.AnyAsync(u => u.UnidadID == dto.UnidadID.Value);
                if (!unidadExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"La unidad con ID {dto.UnidadID} no existe"
                    });
                }
            }

            // Generar código único de incidencia
            var codigoIncidencia = await GenerarCodigoIncidencia();

            // Crear la incidencia
            var incidencia = new Incidencia
            {
                CodigoIncidencia = codigoIncidencia,
                TipoIncidenciaID = dto.TipoIncidenciaID,
                ViajeID = dto.ViajeID,
                UnidadID = dto.UnidadID,
                ReportadoPor = userId,
                Titulo = dto.Titulo.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                Prioridad = dto.Prioridad,
                FechaReporte = DateTime.Now,
                Estatus = 1 // 1 = Abierta (debe existir en EstatusGeneral)
            };

            _context.Incidencias.Add(incidencia);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Incidencia {CodigoIncidencia} creada por el usuario {UserId}. Tipo: {TipoIncidenciaID}, Prioridad: {Prioridad}",
                codigoIncidencia, userId, dto.TipoIncidenciaID, dto.Prioridad);

            // Cargar las relaciones para la respuesta
            await _context.Entry(incidencia).Reference(i => i.TipoIncidencia).LoadAsync();
            await _context.Entry(incidencia).Reference(i => i.Reportador).LoadAsync();
            await _context.Entry(incidencia).Reference(i => i.EstatusNavigation).LoadAsync();

            if (incidencia.ViajeID.HasValue)
            {
                await _context.Entry(incidencia).Reference(i => i.Viaje).LoadAsync();
            }

            if (incidencia.UnidadID.HasValue)
            {
                await _context.Entry(incidencia).Reference(i => i.Unidad).LoadAsync();
            }

            var response = MapearAIncidenciaResponse(incidencia);

            return CreatedAtAction(
                nameof(GetIncidencia), 
                new { id = incidencia.IncidenciaID }, 
                new
                {
                    success = true,
                    message = "Incidencia creada exitosamente",
                    data = response
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la incidencia: {@Dto}", dto);
            return StatusCode(500, new
            {
                success = false,
                message = "Error al crear la incidencia",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtener incidencias de un viaje específico
    /// GET: api/incidencias/viaje/{viajeId}
    /// </summary>
    /// <remarks>
    /// Usado por el staff para ver incidencias relacionadas con su viaje actual.
    /// </remarks>
    [HttpGet("viaje/{viajeId}")]
    [ClRequirePermission(AppPermissions.Incidencias.View)]
    [ProducesResponseType(typeof(List<IncidenciaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetIncidenciasPorViaje(int viajeId)
    {
        try
        {
            // Verificar que el viaje existe
            var viajeExiste = await _context.Viajes.AnyAsync(v => v.ViajeID == viajeId);
            if (!viajeExiste)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"El viaje con ID {viajeId} no existe"
                });
            }

            var incidencias = await _context.Incidencias
                .Include(i => i.TipoIncidencia)
                .Include(i => i.Reportador)
                .Include(i => i.Asignado)
                .Include(i => i.EstatusNavigation)
                .Include(i => i.Unidad)
                .Where(i => i.ViajeID == viajeId)
                .OrderByDescending(i => i.FechaReporte)
                .ToListAsync();

            var response = incidencias.Select(i => MapearAIncidenciaResponse(i)).ToList();

            return Ok(new
            {
                success = true,
                data = response,
                total = response.Count,
                viajeId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener incidencias del viaje {ViajeId}", viajeId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error al obtener las incidencias del viaje",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtener mis incidencias reportadas (incidencias del usuario actual)
    /// GET: api/incidencias/mis-reportes
    /// </summary>
    [HttpGet("mis-reportes")]
    [ProducesResponseType(typeof(List<IncidenciaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMisIncidencias()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Usuario no autenticado"
                });
            }

            var incidencias = await _context.Incidencias
                .Include(i => i.TipoIncidencia)
                .Include(i => i.Reportador)
                .Include(i => i.Asignado)
                .Include(i => i.EstatusNavigation)
                .Include(i => i.Viaje)
                .Include(i => i.Unidad)
                .Where(i => i.ReportadoPor == userId)
                .OrderByDescending(i => i.FechaReporte)
                .ToListAsync();

            var response = incidencias.Select(i => MapearAIncidenciaResponse(i)).ToList();

            return Ok(new
            {
                success = true,
                data = response,
                total = response.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las incidencias del usuario");
            return StatusCode(500, new
            {
                success = false,
                message = "Error al obtener sus incidencias",
                error = ex.Message
            });
        }
    }

    #endregion

    #region Endpoints de Catálogos

    /// <summary>
    /// Obtener todos los tipos de incidencia activos (catálogo)
    /// GET: api/incidencias/tipos
    /// </summary>
    [HttpGet("tipos")]
    [ProducesResponseType(typeof(List<TipoIncidenciaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTiposIncidencia()
    {
        try
        {
            var tipos = await _context.TipoIncidencia
                .Where(t => t.EsActivo)
                .Select(t => new TipoIncidenciaDto
                {
                    TipoIncidenciaID = t.TipoIncidenciaID,
                    Codigo = t.Codigo,
                    Nombre = t.Nombre,
                    Categoria = t.Categoria,
                    Prioridad = t.Prioridad,
                    EsActivo = t.EsActivo
                })
                .OrderBy(t => t.Categoria)
                .ThenBy(t => t.Nombre)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = tipos,
                total = tipos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los tipos de incidencia");
            return StatusCode(500, new
            {
                success = false,
                message = "Error al obtener los tipos de incidencia",
                error = ex.Message
            });
        }
    }

    #endregion

    #region Métodos Auxiliares Privados

    /// <summary>
    /// Mapea una entidad Incidencia a su DTO de respuesta
    /// </summary>
    private static IncidenciaResponseDto MapearAIncidenciaResponse(Incidencia i)
    {
        var tiempoTranscurrido = CalcularTiempoTranscurrido(i.FechaReporte);

        return new IncidenciaResponseDto
        {
            IncidenciaID = i.IncidenciaID,
            CodigoIncidencia = i.CodigoIncidencia,
            TipoIncidenciaID = i.TipoIncidenciaID,
            TipoIncidenciaNombre = i.TipoIncidencia.Nombre,
            TipoIncidenciaCategoria = i.TipoIncidencia.Categoria ?? "",
            ViajeID = i.ViajeID,
            ViajeCodigoViaje = i.Viaje?.CodigoViaje,
            UnidadID = i.UnidadID,
            UnidadPlacas = i.Unidad?.Placas,
            ReportadoPor = i.ReportadoPor,
            ReportadorNombre = i.Reportador.NombreCompleto ?? i.Reportador.UserName ?? "",
            ReportadorEmail = i.Reportador?.Email,
            Titulo = i.Titulo,
            Descripcion = i.Descripcion,
            Prioridad = i.Prioridad,
            FechaReporte = i.FechaReporte,
            FechaResolucion = i.FechaResolucion,
            Estatus = i.Estatus,
            EstatusNombre = i.EstatusNavigation.Nombre,
            EstatusCodigo = i.EstatusNavigation.Codigo,
            AsignadoA = i.AsignadoA,
            AsignadoNombre = i.Asignado != null ? (i.Asignado.NombreCompleto ?? i.Asignado.UserName) : null,
            AsignadoEmail = i.Asignado?.Email,
            TiempoTranscurrido = tiempoTranscurrido
        };
    }

    /// <summary>
    /// Calcula el tiempo transcurrido desde una fecha de forma legible
    /// </summary>
    private static string CalcularTiempoTranscurrido(DateTime fechaInicio)
    {
        var diferencia = DateTime.Now - fechaInicio;

        if (diferencia.TotalMinutes < 60)
            return $"Hace {(int)diferencia.TotalMinutes} minutos";

        if (diferencia.TotalHours < 24)
            return $"Hace {(int)diferencia.TotalHours} horas";

        if (diferencia.TotalDays < 7)
            return $"Hace {(int)diferencia.TotalDays} días";

        if (diferencia.TotalDays < 30)
            return $"Hace {(int)(diferencia.TotalDays / 7)} semanas";

        return $"Hace {(int)(diferencia.TotalDays / 30)} meses";
    }

    /// <summary>
    /// Genera un código único de incidencia con formato: INC-YYYYMMDD-0001
    /// </summary>
    private async Task<string> GenerarCodigoIncidencia()
    {
        var fecha = DateTime.Now.ToString("yyyyMMdd");
        var prefijo = $"INC-{fecha}";

        var ultimaIncidencia = await _context.Incidencias
            .Where(i => i.CodigoIncidencia.StartsWith(prefijo))
            .OrderByDescending(i => i.CodigoIncidencia)
            .FirstOrDefaultAsync();

        int consecutivo = 1;
        if (ultimaIncidencia != null)
        {
            var partes = ultimaIncidencia.CodigoIncidencia.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out int numero))
            {
                consecutivo = numero + 1;
            }
        }

        return $"{prefijo}-{consecutivo:D4}";
    }

    /// <summary>
    /// Aplica ordenamiento dinámico a la consulta de incidencias
    /// </summary>
    private static IQueryable<Incidencia> AplicarOrdenamiento(
        IQueryable<Incidencia> query,
        string? ordenarPor,
        string? direccion)
    {
        var ascendente = direccion?.ToLower() == "asc";

        return (ordenarPor?.ToLower()) switch
        {
            "codigo" => ascendente 
                ? query.OrderBy(i => i.CodigoIncidencia) 
                : query.OrderByDescending(i => i.CodigoIncidencia),
            "titulo" => ascendente 
                ? query.OrderBy(i => i.Titulo) 
                : query.OrderByDescending(i => i.Titulo),
            "prioridad" => ascendente 
                ? query.OrderBy(i => i.Prioridad) 
                : query.OrderByDescending(i => i.Prioridad),
            "estatus" => ascendente 
                ? query.OrderBy(i => i.Estatus) 
                : query.OrderByDescending(i => i.Estatus),
            "tipo" => ascendente 
                ? query.OrderBy(i => i.TipoIncidencia.Nombre) 
                : query.OrderByDescending(i => i.TipoIncidencia.Nombre),
            _ => query.OrderByDescending(i => i.FechaReporte) // Default: más recientes primero
        };
    }

    #endregion
}

