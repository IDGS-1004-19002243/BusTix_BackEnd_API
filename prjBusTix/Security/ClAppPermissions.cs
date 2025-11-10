﻿namespace prjBusTix.Security
{
    public static class ClAppPermissions
    {
        public const string Wildcard = "*";

        // ===== EVENTOS =====
        public const string EventosView = "eventos:view";
        public const string EventosCreate = "eventos:create";
        public const string EventosUpdate = "eventos:update";
        public const string EventosDelete = "eventos:delete";

        // ===== VIAJES =====
        public const string ViajesView = "viajes:view";
        public const string ViajesCreate = "viajes:create";
        public const string ViajesUpdate = "viajes:update";
        public const string ViajesDelete = "viajes:delete";

        // ===== BOLETOS =====
        public const string BoletosView = "boletos:view";
        public const string BoletosCreate = "boletos:create";
        public const string BoletosValidate = "boletos:validate";

        // ===== PAGOS =====
        public const string PagosView = "pagos:view";
        public const string PagosManage = "pagos:manage";

        // ===== INCIDENCIAS =====
        public const string IncidenciasView = "incidencias:view";
        public const string IncidenciasCreate = "incidencias:create";
        public const string IncidenciasUpdate = "incidencias:update";

        // ===== NOTIFICACIONES =====
        public const string NotificacionesView = "notificaciones:view";
        public const string NotificacionesCreate = "notificaciones:create";
        public const string NotificacionesBroadcast = "notificaciones:broadcast";

        // ===== RUTAS Y PLANTILLAS =====
        public const string RutasView = "rutas:view";
        public const string RutasCreate = "rutas:create";
        public const string RutasUpdate = "rutas:update";
        public const string RutasDelete = "rutas:delete";

        // ===== UNIDADES =====
        public const string UnidadesView = "unidades:view";
        public const string UnidadesCreate = "unidades:create";
        public const string UnidadesUpdate = "unidades:update";
        public const string UnidadesDelete = "unidades:delete";

        // ===== USUARIOS =====
        public const string UsersView = "users:view";
        public const string UsersCreate = "users:create";
        public const string UsersUpdate = "users:update";
        public const string UsersDelete = "users:delete";

        // ===== ROLES =====
        public const string RolesView = "roles:view";
        public const string RolesCreate = "roles:create";
        public const string RolesUpdate = "roles:update";
        public const string RolesDelete = "roles:delete";
        public const string RolesAssignPermissions = "roles:assign_permissions";
        public const string RolesAssignUsers = "roles:assign_users";

        // ===== AUDITORÍA =====
        public const string AuditoriaView = "auditoria:view";

        // ===== MAPEO DE ROLES A PERMISOS =====
        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            // SUPER ADMIN - Acceso total
            ["SuperAdmin"] = new[] { Wildcard },
            
            // ADMIN - Gestión completa del sistema
            ["Admin"] = new[]
            {
                EventosView, EventosCreate, EventosUpdate, EventosDelete,
                ViajesView, ViajesCreate, ViajesUpdate, ViajesDelete,
                BoletosView, BoletosCreate, BoletosValidate,
                PagosView, PagosManage,
                IncidenciasView, IncidenciasCreate, IncidenciasUpdate,
                NotificacionesView, NotificacionesCreate, NotificacionesBroadcast,
                RutasView, RutasCreate, RutasUpdate, RutasDelete,
                UnidadesView, UnidadesCreate, UnidadesUpdate, UnidadesDelete,
                UsersView, UsersCreate, UsersUpdate, UsersDelete,
                RolesView, RolesCreate, RolesUpdate, RolesDelete, RolesAssignPermissions, RolesAssignUsers,
                AuditoriaView
            },

            // COORDINADOR - Gestión operativa
            ["Coordinador"] = new[]
            {
                EventosView, EventosCreate, EventosUpdate,
                ViajesView, ViajesCreate, ViajesUpdate,
                BoletosView, BoletosValidate,
                PagosView,
                IncidenciasView, IncidenciasUpdate,
                NotificacionesView, NotificacionesCreate, NotificacionesBroadcast,
                RutasView,
                UnidadesView,
                UsersView
            },

            // STAFF - Personal operativo
            ["Staff"] = new[]
            {
                ViajesView,
                BoletosView, BoletosValidate,
                IncidenciasView, IncidenciasCreate,
                NotificacionesView
            },

            // CLIENTE - Usuario final
            ["Cliente"] = new[]
            {
                EventosView,
                ViajesView,
                BoletosView, BoletosCreate,
                NotificacionesView
            }
        };
    }
}
