using Microsoft.AspNetCore.Http;
using MultiTenantSaaS.Infrastructure.Services;
using System.Security.Claims;

namespace MultiTenantSaaS.Infrastructure.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Extract tenant ID and user ID from JWT claims
        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            tenantService.SetTenantContext(tenantId, userIdClaim ?? "system");
        }

        await _next(context);
    }
}
