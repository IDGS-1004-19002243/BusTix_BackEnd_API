using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace prjBusTix.Security
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var user = context.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return Task.CompletedTask;

            var permissions = user.Claims
                .Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value?.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (permissions.Contains("*") || permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var colonIndex = requirement.Permission.IndexOf(':');
            if (colonIndex > 0)
            {
                var module = requirement.Permission.Substring(0, colonIndex);
                if (permissions.Contains($"{module}:*"))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
    }
}

