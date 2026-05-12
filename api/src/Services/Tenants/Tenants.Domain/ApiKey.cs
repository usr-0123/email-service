using EmailPlatform.BuildingBlocks.Persistence.Mongo;

namespace EmailPlatform.Services.Tenants.Domain;

public class ApiKey : ITenantScoped
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt is null;

    public static ApiKey Create(string tenantId, string name, string keyHash, string prefix)
    {
        return new ApiKey
        {
            TenantId = tenantId,
            Name = name,
            KeyHash = keyHash,
            Prefix = prefix,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
