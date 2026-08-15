using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using System.Security.Claims;

namespace SaaSFoundry.Plugins.Authentication.Extensions;

public class ContextPopulationMiddleware
{
    private readonly RequestDelegate _next;

    public ContextPopulationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var subjectId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            var identityType = context.User.FindFirst("IdentityType")?.Value ?? "User";
            var tenantId = context.User.FindFirst("TenantId")?.Value ?? string.Empty;

            var claims = context.User.Claims
                .Where(c => c.Type != ClaimTypes.NameIdentifier && c.Type != "IdentityType" && c.Type != "TenantId")
                .ToDictionary(c => c.Type, c => c.Value);

            var identityContext = new IdentityContext(subjectId, identityType, claims, tenantId);
            var tenantContext = new TenantContext(tenantId);
            
            var authScheme = context.User.Identity.AuthenticationType ?? "Unknown";
            var authContext = new AuthenticationContext(authScheme, "Authenticated");

            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = context.User.FindAll("Permission").Select(c => c.Value).ToList();
            
            var authorizationContext = new AuthorizationContext(permissions, roles);

            context.Features.Set(identityContext);
            context.Features.Set(tenantContext);
            context.Features.Set(authContext);
            context.Features.Set(authorizationContext);
        }

        await _next(context);
    }
}
