using Microsoft.AspNetCore.Authentication;

namespace EmailPlatform.BuildingBlocks.Authentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string Scheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";
}
