namespace EmailPlatform.Services.Tenants.Domain;

public class Tenant
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string HomeRegion { get; set; } = "ke-1";
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public SenderConfig SenderConfig { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static Tenant Create(string name, string slug, string homeRegion = "ke-1")
    {
        var now = DateTime.UtcNow;
        return new Tenant
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            HomeRegion = homeRegion,
            Status = TenantStatus.Active,
            SenderConfig = SenderConfig.UseShared(),
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}

public enum TenantStatus
{
    Active,
    Suspended,
    Archived,
}
