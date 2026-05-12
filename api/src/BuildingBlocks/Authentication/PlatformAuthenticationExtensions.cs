using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EmailPlatform.BuildingBlocks.Authentication;

public static class PlatformAuthenticationExtensions
{
    public static IServiceCollection AddPlatformAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableApiKeyScheme = true)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                $"Missing '{JwtOptions.SectionName}' configuration section.");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var authBuilder = services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
            {
                opt.Authority = jwt.Authority;
                opt.Audience = jwt.Audience;
                opt.RequireHttpsMetadata = jwt.RequireHttpsMetadata;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(jwt.Issuer),
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(jwt.Audience),
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        if (enableApiKeyScheme)
        {
            authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.Scheme, _ => { });
        }

        services.AddAuthorization(options =>
        {
            var schemes = enableApiKeyScheme
                ? new[] { JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationOptions.Scheme }
                : new[] { JwtBearerDefaults.AuthenticationScheme };

            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(schemes)
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
