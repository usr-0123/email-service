using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmailPlatform.BuildingBlocks.Authentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyValidator _validator;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidator validator)
        : base(options, logger, encoder)
    {
        _validator = validator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var values))
        {
            return AuthenticateResult.NoResult();
        }

        var presented = values.ToString();
        if (string.IsNullOrWhiteSpace(presented))
        {
            return AuthenticateResult.NoResult();
        }

        var result = await _validator.ValidateAsync(presented, Context.RequestAborted);
        if (!result.IsValid || result.TenantId is null || result.KeyId is null)
        {
            return AuthenticateResult.Fail("Invalid API key");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.KeyId),
            new(PlatformClaimTypes.TenantId, result.TenantId),
            new(PlatformClaimTypes.ApiKeyId, result.KeyId),
            new(PlatformClaimTypes.ApiKeyName, result.KeyName ?? string.Empty),
            new(ClaimTypes.AuthenticationMethod, ApiKeyAuthenticationOptions.Scheme),
        };

        var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationOptions.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.Scheme);
        return AuthenticateResult.Success(ticket);
    }
}
