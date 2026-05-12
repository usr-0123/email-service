namespace EmailPlatform.Services.Identity.Infrastructure;

public sealed class IdentityJwtOptions
{
    public const string SectionName = "IdentityJwt";

    public string Issuer { get; set; } = "emailplatform.identity";
    public string TenantAudience { get; set; } = "emailplatform.tenant";
    public string AdminAudience { get; set; } = "emailplatform.admin";
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(60);

    public string TenantKeyId { get; set; } = "ep-tenant-1";
    public string AdminKeyId { get; set; } = "ep-admin-1";

    public string TenantRsaPrivateKeyPem { get; set; } = string.Empty;
    public string AdminRsaPrivateKeyPem { get; set; } = string.Empty;
}
