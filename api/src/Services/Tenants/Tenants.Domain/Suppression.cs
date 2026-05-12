using EmailPlatform.BuildingBlocks.Persistence.Mongo;

namespace EmailPlatform.Services.Tenants.Domain;

public class Suppression : ITenantScoped
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public SuppressionReason Reason { get; set; }
    public DateTime CreatedAt { get; set; }

    public static Suppression Create(string tenantId, string email, SuppressionReason reason)
    {
        return new Suppression
        {
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            Reason = reason,
            CreatedAt = DateTime.UtcNow,
        };
    }
}

public enum SuppressionReason
{
    Unsubscribed,
    HardBounce,
    SpamComplaint,
    ManualBlock,
}
