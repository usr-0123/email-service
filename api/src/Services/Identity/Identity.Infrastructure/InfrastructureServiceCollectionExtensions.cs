using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Identity.Application.Abstractions;
using EmailPlatform.Services.Identity.Infrastructure.Auth;
using EmailPlatform.Services.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailPlatform.Services.Identity.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongo(configuration);

        services.Configure<IdentityJwtOptions>(configuration.GetSection(IdentityJwtOptions.SectionName));
        services.AddSingleton<RsaKeyProvider>();
        services.AddSingleton<IJwtIssuer, RsaJwtIssuer>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IClock, SystemClock>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddHostedService<IdentityIndexInitializer>();

        return services;
    }
}
