using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmailPlatform.Services.Identity.Infrastructure.Auth;

public sealed class RsaKeyProvider : IDisposable
{
    private readonly IdentityJwtOptions _options;
    private readonly RSA _tenantRsa;
    private readonly RSA _adminRsa;

    public RsaKeyProvider(IOptions<IdentityJwtOptions> options, ILogger<RsaKeyProvider> logger)
    {
        _options = options.Value;
        _tenantRsa = LoadOrGenerate(_options.TenantRsaPrivateKeyPem, "tenant", logger);
        _adminRsa = LoadOrGenerate(_options.AdminRsaPrivateKeyPem, "admin", logger);

        TenantKey = new RsaSecurityKey(_tenantRsa) { KeyId = _options.TenantKeyId };
        AdminKey = new RsaSecurityKey(_adminRsa) { KeyId = _options.AdminKeyId };
    }

    public RsaSecurityKey TenantKey { get; }
    public RsaSecurityKey AdminKey { get; }

    public JsonWebKeySet GetPublicJwks()
    {
        var jwks = new JsonWebKeySet();
        jwks.Keys.Add(BuildPublicJwk(_tenantRsa, _options.TenantKeyId));
        jwks.Keys.Add(BuildPublicJwk(_adminRsa, _options.AdminKeyId));
        return jwks;
    }

    public void Dispose()
    {
        _tenantRsa.Dispose();
        _adminRsa.Dispose();
    }

    private static JsonWebKey BuildPublicJwk(RSA rsa, string kid)
    {
        var p = rsa.ExportParameters(includePrivateParameters: false);
        return new JsonWebKey
        {
            Kty = "RSA",
            Use = "sig",
            Kid = kid,
            Alg = SecurityAlgorithms.RsaSha256,
            N = Base64UrlEncoder.Encode(p.Modulus!),
            E = Base64UrlEncoder.Encode(p.Exponent!),
        };
    }

    private static RSA LoadOrGenerate(string pem, string label, ILogger logger)
    {
        var rsa = RSA.Create();
        if (string.IsNullOrWhiteSpace(pem))
        {
            rsa.KeySize = 2048;
            logger.LogWarning(
                "No {Label} RSA private key configured. Generated an ephemeral 2048-bit key — tokens will invalidate on restart. Configure 'IdentityJwt:{Label}RsaPrivateKeyPem' for stable keys.",
                label, label);
        }
        else
        {
            rsa.ImportFromPem(pem);
        }
        return rsa;
    }
}
