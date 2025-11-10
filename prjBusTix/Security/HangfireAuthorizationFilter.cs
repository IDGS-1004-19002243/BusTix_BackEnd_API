using Hangfire.Dashboard;

namespace prjBusTix.Security;

/// <summary>
/// Filtro de autorización para el dashboard de Hangfire
/// Solo permite acceso a usuarios con rol Admin
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Permitir acceso solo si está autenticado y tiene rol Admin
        return httpContext.User.Identity?.IsAuthenticated == true 
               && httpContext.User.IsInRole("Admin");
    }
}

