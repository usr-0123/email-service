using Microsoft.IdentityModel.Tokens;

namespace EmailPlatform.Services.Identity.Application.Abstractions;

public interface IJwtIssuer
{
    IssuedToken IssueUserAccessToken(
        string userId,
        string tenantId,
        string email,
        IEnumerable<string> roles,
        DateTime issuedAtUtc);

    IssuedToken IssueAdminAccessToken(
        string adminId,
        string email,
        IEnumerable<string> roles,
        DateTime issuedAtUtc);

    JsonWebKeySet GetPublicJsonWebKeySet();

    OpenIdConfiguration GetOpenIdConfiguration(string issuer);
}

public sealed record IssuedToken(string Token, DateTime ExpiresAtUtc);

public sealed record OpenIdConfiguration(
    string Issuer,
    string JwksUri,
    IReadOnlyList<string> IdTokenSigningAlgValuesSupported);
