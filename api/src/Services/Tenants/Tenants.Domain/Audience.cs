using EmailPlatform.BuildingBlocks.Persistence.Mongo;

namespace EmailPlatform.Services.Tenants.Domain;

public class Audience : ITenantScoped
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static Audience Create(string tenantId, string name, string? description = null)
    {
        var now = DateTime.UtcNow;
        return new Audience
        {
            TenantId = tenantId,
            Name = name,
            Description = description,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}
