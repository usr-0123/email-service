using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmailPlatform.BuildingBlocks.Authentication;
using EmailPlatform.Services.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmailPlatform.Services.Identity.Infrastructure.Auth;

internal sealed class RsaJwtIssuer : IJwtIssuer
{
    private readonly RsaKeyProvider _keys;
    private readonly IdentityJwtOptions _options;
    private readonly JwtSecurityTokenHandler _handler = new();

    public RsaJwtIssuer(RsaKeyProvider keys, IOptions<IdentityJwtOptions> options)
    {
        _keys = keys;
        _options = options.Value;
    }

    public IssuedToken IssueUserAccessToken(
        string userId,
        string tenantId,
        string email,
        IEnumerable<string> roles,
        DateTime issuedAtUtc)
    {
        var expires = issuedAtUtc.Add(_options.AccessTokenLifetime);

        var claims = BaseClaims(userId, email, roles);
        claims.Add(new Claim(PlatformClaimTypes.TenantId, tenantId));

        var token = CreateToken(claims, _options.TenantAudience, _keys.TenantKey, issuedAtUtc, expires);
        return new IssuedToken(token, expires);
    }

    public IssuedToken IssueAdminAccessToken(
        string adminId,
        string email,
        IEnumerable<string> roles,
        DateTime issuedAtUtc)
    {
        var expires = issuedAtUtc.Add(_options.AccessTokenLifetime);
        var claims = BaseClaims(adminId, email, roles);
        var token = CreateToken(claims, _options.AdminAudience, _keys.AdminKey, issuedAtUtc, expires);
        return new IssuedToken(token, expires);
    }

    public JsonWebKeySet GetPublicJsonWebKeySet() => _keys.GetPublicJwks();

    public OpenIdConfiguration GetOpenIdConfiguration(string issuer) => new(
        Issuer: issuer,
        JwksUri: $"{issuer.TrimEnd('/')}/.well-known/jwks.json",
        IdTokenSigningAlgValuesSupported: new[] { SecurityAlgorithms.RsaSha256 });

    private static List<Claim> BaseClaims(string subjectId, string email, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subjectId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        return claims;
    }

    private string CreateToken(
        IEnumerable<Claim> claims,
        string audience,
        RsaSecurityKey key,
        DateTime notBefore,
        DateTime expires)
    {
        var signing = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: audience,
            claims: claims,
            notBefore: notBefore,
            expires: expires,
            signingCredentials: signing);
        return _handler.WriteToken(jwt);
    }
}
