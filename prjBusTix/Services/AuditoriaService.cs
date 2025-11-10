using Microsoft.EntityFrameworkCore;
using prjBusTix.Data;
using prjBusTix.Model;
using System.Security.Claims;
using System.Text.Json;

namespace prjBusTix.Services;

/// <summary>
/// Servicio para gestión de auditoría de cambios
/// </summary>
public class AuditoriaService : IAuditoriaService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditoriaService> _logger;

    public AuditoriaService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditoriaService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Registra un cambio en la auditoría
    /// </summary>
    public async Task RegistrarCambioAsync(
        string tabla,
        string registroId,
        string accion,
        string? datosPrevios = null,
        string? datosPosteriores = null)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier);

            var auditoria = new AuditoriaCambio
            {
                TablaAfectada = tabla,
                RegistroID = registroId,
                UsuarioID = userId,
                TipoOperacion = accion,
                FechaHoraCambio = DateTime.Now,
                ValoresAnteriores = datosPrevios,
                ValoresNuevos = datosPosteriores
            };

            _context.AuditoriaCambios.Add(auditoria);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Auditoría registrada: {Tabla}.{RegistroId} - {Accion} por {Usuario}",
                tabla, registroId, accion, userId ?? "SYSTEM");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error al registrar auditoría para {Tabla}.{RegistroId}", 
                tabla, registroId);
        }
    }

    /// <summary>
    /// Obtiene el historial de auditoría con filtros
    /// </summary>
    public async Task<List<AuditoriaCambio>> ObtenerHistorialAsync(
        string? tabla = null,
        string? usuarioId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        int pagina = 1,
        int tamanoPagina = 50)
    {
        try
        {
            var query = _context.AuditoriaCambios
                .Include(a => a.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tabla))
                query = query.Where(a => a.TablaAfectada == tabla);

            if (!string.IsNullOrWhiteSpace(usuarioId))
                query = query.Where(a => a.UsuarioID == usuarioId);

            if (fechaDesde.HasValue)
                query = query.Where(a => a.FechaHoraCambio >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.FechaHoraCambio <= fechaHasta.Value);

            return await query
                .OrderByDescending(a => a.FechaHoraCambio)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de auditoría");
            return new List<AuditoriaCambio>();
        }
    }

    /// <summary>
    /// Obtiene el historial de auditoría de un registro específico
    /// </summary>
    public async Task<List<AuditoriaCambio>> ObtenerHistorialRegistroAsync(
        string tabla, 
        string registroId)
    {
        try
        {
            return await _context.AuditoriaCambios
                .Include(a => a.Usuario)
                .Where(a => a.TablaAfectada == tabla && a.RegistroID == registroId)
                .OrderByDescending(a => a.FechaHoraCambio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error al obtener historial de {Tabla}.{RegistroId}", 
                tabla, registroId);
            return new List<AuditoriaCambio>();
        }
    }

    /// <summary>
    /// Serializa un objeto a JSON para auditoría
    /// </summary>
    public static string SerializarObjeto(object? obj)
    {
        if (obj == null) return string.Empty;

        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
        catch
        {
            return obj.ToString() ?? string.Empty;
        }
    }
}

