using Microsoft.AspNetCore.Http;

namespace EmailPlatform.BuildingBlocks.Multitenancy;

public sealed class TenantResolutionMiddleware
{
    public const string TenantIdClaimType = "tid";

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.User?.FindFirst(TenantIdClaimType)?.Value;

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            context.Items[TenantContextAccessor.TenantIdItemKey] = tenantId;
        }

        await _next(context);
    }
}
