using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace prjBusTix.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real
/// Los clientes se conectan aquí para recibir notificaciones push instantáneas
/// </summary>
[Authorize]
public class NotificacionesHub : Hub
{
    private readonly ILogger<NotificacionesHub> _logger;
    
    // Diccionario para mantener el mapeo usuario -> connectionId
    private static readonly Dictionary<string, List<string>> _connections = new();
    private static readonly object _lock = new();

    public NotificacionesHub(ILogger<NotificacionesHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se conecta al hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!string.IsNullOrEmpty(userId))
        {
            lock (_lock)
            {
                if (!_connections.ContainsKey(userId))
                {
                    _connections[userId] = new List<string>();
                }
                
                _connections[userId].Add(Context.ConnectionId);
            }
            
            _logger.LogInformation(
                "Usuario {UserId} conectado al hub. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
            
            // Agregar al grupo del usuario
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se desconecta del hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!string.IsNullOrEmpty(userId))
        {
            lock (_lock)
            {
                if (_connections.ContainsKey(userId))
                {
                    _connections[userId].Remove(Context.ConnectionId);
                    
                    if (_connections[userId].Count == 0)
                    {
                        _connections.Remove(userId);
                    }
                }
            }
            
            _logger.LogInformation(
                "Usuario {UserId} desconectado del hub. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Permite al cliente unirse a un grupo específico (por ejemplo, viaje)
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation(
            "ConnectionId {ConnectionId} se unió al grupo {GroupName}",
            Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Permite al cliente salir de un grupo específico
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation(
            "ConnectionId {ConnectionId} salió del grupo {GroupName}",
            Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Obtiene todos los ConnectionIds de un usuario específico
    /// </summary>
    public static List<string> GetConnectionIds(string userId)
    {
        lock (_lock)
        {
            return _connections.ContainsKey(userId) 
                ? new List<string>(_connections[userId]) 
                : new List<string>();
        }
    }

    /// <summary>
    /// Verifica si un usuario está conectado
    /// </summary>
    public static bool IsUserConnected(string userId)
    {
        lock (_lock)
        {
            return _connections.ContainsKey(userId) && _connections[userId].Any();
        }
    }

    /// <summary>
    /// Obtiene el número total de usuarios conectados
    /// </summary>
    public static int GetTotalConnectedUsers()
    {
        lock (_lock)
        {
            return _connections.Count;
        }
    }

    /// <summary>
    /// Obtiene el número total de conexiones activas
    /// </summary>
    public static int GetTotalConnections()
    {
        lock (_lock)
        {
            return _connections.Values.Sum(list => list.Count);
        }
    }
}

