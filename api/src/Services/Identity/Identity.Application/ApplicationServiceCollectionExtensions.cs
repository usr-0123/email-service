using EmailPlatform.Services.Identity.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace EmailPlatform.Services.Identity.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginUserHandler>();
        services.AddScoped<LoginAdminHandler>();
        services.AddScoped<RefreshTokensHandler>();
        return services;
    }
}
