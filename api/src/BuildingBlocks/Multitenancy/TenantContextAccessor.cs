using Microsoft.AspNetCore.Http;

namespace EmailPlatform.BuildingBlocks.Multitenancy;

internal sealed class TenantContextAccessor : ITenantContext
{
    internal const string TenantIdItemKey = "TenantId";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextAccessor(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string? CurrentTenantId
        => _httpContextAccessor.HttpContext?.Items[TenantIdItemKey] as string;

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
