using System;
using Microsoft.AspNetCore.Authorization;

namespace prjBusTix.Security
{
    /// <summary>
    /// Atributo para requerir permisos en controladores/acciones.
    /// Uso: [RequirePermission("module:action")] o [RequirePermission("*")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RequirePermissionAttribute : AuthorizeAttribute
    {
        private const string POLICY_PREFIX = "Permission:";

        /// <summary>
        /// Crea una nueva instancia del atributo con el permiso requerido.
        /// </summary>
        /// <param name="permission">Permiso en formato "modulo:accion" o "*" para acceso total.</param>
        public RequirePermissionAttribute(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("El permiso no puede estar vacío.", nameof(permission));

            Policy = POLICY_PREFIX + permission.Trim();
        }

    }
}