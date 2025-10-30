namespace AuthAPI.Security
{
    public static class AppPermissions
    {
        public const string Wildcard = "*";

        // ===== EVENTOS MASIVOS =====
        public static class Events
        {
            public const string Read = "events:read";
            public const string Create = "events:create";
            public const string Update = "events:update";
            public const string Delete = "events:delete";
            public const string All = "events:*";
        }

        // ===== RUTAS =====
        public static class Routes
        {
            public const string Read = "routes:read";
            public const string Create = "routes:create";
            public const string Update = "routes:update";
            public const string Delete = "routes:delete";
            public const string All = "routes:*";
        }

        // ===== BOLETOS =====
        public static class Tickets
        {
            public const string Read = "tickets:read";
            public const string ReadOwn = "tickets:read_own";
            public const string Purchase = "tickets:purchase";
            public const string Validate = "tickets:validate";
            public const string All = "tickets:*";
        }

        // ===== COTIZACIONES =====
        public static class Quotes
        {
            public const string Read = "quotes:read";
            public const string ReadOwn = "quotes:read_own";
            public const string Create = "quotes:create";
            public const string Respond = "quotes:respond";
            public const string Approve = "quotes:approve";
            public const string All = "quotes:*";
        }

        // ===== RESERVAS/BOOKINGS =====
        public static class Bookings
        {
            public const string Read = "bookings:read";
            public const string ReadOwn = "bookings:read_own";
            public const string ReadAssigned = "bookings:read_assigned";
            public const string Create = "bookings:create";
            public const string Update = "bookings:update";
            public const string ModifyOwn = "bookings:modify_own";
            public const string Cancel = "bookings:cancel";
            public const string CancelOwn = "bookings:cancel_own";
            public const string All = "bookings:*";
        }

        // ===== CONTRATOS =====
        public static class Contracts
        {
            public const string Read = "contracts:read";
            public const string ReadOwn = "contracts:read_own";
            public const string Create = "contracts:create";
            public const string Sign = "contracts:sign";
            public const string All = "contracts:*";
        }

        // ===== FLOTA =====
        public static class Fleet
        {
            public const string Read = "fleet:read";
            public const string Manage = "fleet:manage";
            public const string All = "fleet:*";
        }

        // ===== OPERADORES =====
        public static class Operators
        {
            public const string Read = "operators:read";
            public const string Manage = "operators:manage";
            public const string All = "operators:*";
        }

        // ===== MANTENIMIENTO =====
        public static class Maintenance
        {
            public const string Read = "maintenance:read";
            public const string Manage = "maintenance:manage";
            public const string All = "maintenance:*";
        }

        // ===== FINANZAS =====
        public static class Finance
        {
            public const string Read = "finance:read";
            public const string Reports = "finance:reports";
            public const string All = "finance:*";
        }

        // ===== FACTURAS =====
        public static class Invoices
        {
            public const string Read = "invoices:read";
            public const string ReadOwn = "invoices:read_own";
            public const string Create = "invoices:create";
            public const string Request = "invoices:request";
            public const string All = "invoices:*";
        }

        // ===== CLIENTES =====
        public static class Customers
        {
            public const string Read = "customers:read";
            public const string Update = "customers:update";
            public const string All = "customers:*";
        }

        // ===== CORPORATIVOS =====
        public static class Corporate
        {
            public const string Read = "corporate:read";
            public const string Manage = "corporate:manage";
            public const string All = "corporate:*";
        }

        // ===== USUARIOS =====
        public static class Users
        {
            public const string Read = "users:read";
            public const string Create = "users:create";
            public const string Update = "users:update";
            public const string Manage = "users:manage";
            public const string All = "users:*";
        }

        // ===== VALIDACIÓN =====
        public static class Validation
        {
            public const string Scan = "validation:scan";
            public const string Report = "validation:report";
            public const string All = "validation:*";
        }

        // ===== PASAJEROS =====
        public static class Passengers
        {
            public const string Read = "passengers:read";
            public const string All = "passengers:*";
        }

        // ===== ANALYTICS =====
        public static class Analytics
        {
            public const string Read = "analytics:read";
            public const string Own = "analytics:own";
            public const string All = "analytics:*";
        }

        // ===== HORARIOS =====
        public static class Schedule
        {
            public const string Read = "schedule:read";
            public const string Manage = "schedule:manage";
            public const string All = "schedule:*";
        }

        // ===== SERVICIOS =====
        public static class Services
        {
            public const string Read = "services:read";
            public const string UpdateStatus = "services:update_status";
            public const string All = "services:*";
        }

        // ===== VEHÍCULOS =====
        public static class Vehicle
        {
            public const string Read = "vehicle:read";
            public const string Report = "vehicle:report";
            public const string All = "vehicle:*";
        }

        // ===== TRACKING =====
        public static class Tracking
        {
            public const string Update = "tracking:update";
            public const string All = "tracking:*";
        }

        // ===== CALIFICACIONES =====
        public static class Ratings
        {
            public const string ReadOwn = "ratings:read_own";
            public const string All = "ratings:*";
        }

        // ===== PERFIL =====
        public static class Profile
        {
            public const string Read = "profile:read";
            public const string Update = "profile:update";
            public const string All = "profile:*";
        }

        // ===== NOTIFICACIONES =====
        public static class Notifications
        {
            public const string Manage = "notifications:manage";
            public const string All = "notifications:*";
        }

        // ===== CUENTA =====
        public static class Account
        {
            public const string ReadOwn = "account:read_own";
            public const string All = "account:*";
        }

        // ===== RESEÑAS =====
        public static class Reviews
        {
            public const string Create = "reviews:create";
            public const string All = "reviews:*";
        }

        // ===== CONFIGURACIÓN =====
        public static class Settings
        {
            public const string Pricing = "settings:pricing";
            public const string Services = "settings:services";
            public const string System = "settings:system";
            public const string All = "settings:*";
        }

        // ===== MAPEO DE ROLES A PERMISOS =====
        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            // SUPER ADMIN - Acceso total
            ["SuperAdmin"] = new[] { Wildcard },
            
            // ADMIN - Gestión completa del sistema
            ["Admin"] = new[]
            {
                Events.All,
                Routes.All,
                Tickets.All,
                Quotes.All,
                Bookings.All,
                Contracts.All,
                Fleet.All,
                Operators.All,
                Maintenance.All,
                Finance.Read,
                Finance.Reports,
                Invoices.All,
                Customers.Read,
                Customers.Update,
                Corporate.All,
                Analytics.All,
                Settings.Pricing,
                Settings.Services,
                Users.Read,
                Users.Create,
                Users.Update
            },
            
            // STAFF - Validación de boletos y control de accesos
            ["Staff"] = new[]
            {
                Events.Read,
                Routes.Read,
                Tickets.Read,
                Tickets.Validate,
                Passengers.Read,
                Validation.Scan,
                Validation.Report,
                Analytics.Own
            },
            
            // OPERATOR - Gestión de servicios asignados
            ["Operator"] = new[]
            {
                Schedule.Read,
                Services.Read,
                Services.UpdateStatus,
                Vehicle.Read,
                Vehicle.Report,
                Tracking.Update,
                Bookings.ReadAssigned,
                Ratings.ReadOwn
            },
            
            // CUSTOMER - Usuario final para eventos
            ["Customer"] = new[]
            {
                Events.Read,
                Routes.Read,
                Tickets.ReadOwn,
                Tickets.Purchase,
                Profile.Read,
                Profile.Update,
                Notifications.Manage
            },
            
            // CORPORATE - Cotizaciones y viajes privados
            ["Corporate"] = new[]
            {
                Events.Read,
                Routes.Read,
                Quotes.Create,
                Quotes.ReadOwn,
                Quotes.Respond,
                Bookings.Create,
                Bookings.ReadOwn,
                Bookings.ModifyOwn,
                Bookings.CancelOwn,
                Invoices.ReadOwn,
                Invoices.Request,
                Contracts.ReadOwn,
                Contracts.Sign,
                Account.ReadOwn,
                Reviews.Create,
                Profile.Read,
                Profile.Update
            }
        };
    }
}