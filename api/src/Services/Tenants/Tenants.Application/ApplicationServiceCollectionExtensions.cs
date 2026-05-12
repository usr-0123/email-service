using EmailPlatform.Services.Tenants.Application.ApiKeys;
using EmailPlatform.Services.Tenants.Application.Audiences;
using EmailPlatform.Services.Tenants.Application.Recipients;
using EmailPlatform.Services.Tenants.Application.Suppressions;
using EmailPlatform.Services.Tenants.Application.Templates;
using EmailPlatform.Services.Tenants.Application.Tenants;
using Microsoft.Extensions.DependencyInjection;

namespace EmailPlatform.Services.Tenants.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddTenantsApplication(this IServiceCollection services)
    {
        services.AddScoped<TenantService>();
        services.AddScoped<ApiKeyService>();
        services.AddScoped<AudienceService>();
        services.AddScoped<RecipientService>();
        services.AddScoped<TemplateService>();
        services.AddScoped<SuppressionService>();
        return services;
    }
}
