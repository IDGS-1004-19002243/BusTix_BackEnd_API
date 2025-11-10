namespace prjBusTix.Security
{
    public static class AppPermissions
    {
        // Definición de permisos por módulo
        public const string Users_View = "users:view";
        public const string Users_Create = "users:create";
        public const string Users_Edit = "users:edit";
        public const string Users_Delete = "users:delete";

        public const string Roles_View = "roles:view";
        public const string Roles_Create = "roles:create";
        public const string Roles_Edit = "roles:edit";
        public const string Roles_Delete = "roles:delete";

        public const string Tickets_View = "tickets:view";
        public const string Tickets_Create = "tickets:create";
        public const string Tickets_Edit = "tickets:edit";
        public const string Tickets_Delete = "tickets:delete";

        public const string Reports_View = "reports:view";
        public const string Reports_Generate = "reports:generate";

        // Permisos para módulo de Incidencias
        public static class Incidencias
        {
            public const string View = "incidencias:view";
            public const string Create = "incidencias:create";
            public const string Update = "incidencias:update";
            public const string Delete = "incidencias:delete";
        }

        // Mapa de permisos por rol
        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            {
                "Admin", new[]
                {
                    "*" // Admin tiene acceso total
                }
            },
            {
                "User", new[]
                {
                    Users_View,
                    Tickets_View,
                    Tickets_Create
                }
            },
            {
                "Manager", new[]
                {
                    Users_View,
                    Users_Edit,
                    Roles_View,
                    Tickets_View,
                    Tickets_Create,
                    Tickets_Edit,
                    Reports_View,
                    Reports_Generate,
                    Incidencias.View,
                    Incidencias.Update
                }
            },
            {
                "Operator", new[]
                {
                    Tickets_View,
                    Tickets_Create,
                    Tickets_Edit,
                    Users_View
                }
            },
            {
                "Staff", new[]
                {
                    Incidencias.View,
                    Incidencias.Create
                }
            }
        };
    }
}
