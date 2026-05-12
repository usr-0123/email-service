using EmailPlatform.BuildingBlocks.Authentication;
using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Infrastructure.Auth;
using EmailPlatform.Services.Tenants.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailPlatform.Services.Tenants.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddTenantsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongo(configuration);

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IAudienceRepository, AudienceRepository>();
        services.AddScoped<IRecipientRepository, RecipientRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<ISuppressionRepository, SuppressionRepository>();

        services.AddScoped<IApiKeyValidator, MongoApiKeyValidator>();

        services.AddHostedService<TenantsIndexInitializer>();

        return services;
    }
}
