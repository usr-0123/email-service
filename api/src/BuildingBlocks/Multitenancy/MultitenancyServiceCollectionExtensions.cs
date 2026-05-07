using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EmailPlatform.BuildingBlocks.Multitenancy;

public static class MultitenancyServiceCollectionExtensions
{
    public static IServiceCollection AddMultitenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContextAccessor>();
        return services;
    }

    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantResolutionMiddleware>();
}
