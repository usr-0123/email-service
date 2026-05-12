using EmailPlatform.BuildingBlocks.Persistence.Mongo;

namespace EmailPlatform.Services.Tenants.Domain;

public class Recipient : ITenantScoped
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string Locale { get; set; } = "en-US";
    public List<string> AudienceIds { get; set; } = new();
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public RecipientStatus Status { get; set; } = RecipientStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static Recipient Create(
        string tenantId,
        string email,
        string? name,
        string locale,
        IEnumerable<string>? audienceIds,
        IDictionary<string, string>? customFields)
    {
        var now = DateTime.UtcNow;
        return new Recipient
        {
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            Name = name,
            Locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale,
            AudienceIds = audienceIds?.ToList() ?? new List<string>(),
            CustomFields = customFields is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(customFields),
            Status = RecipientStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}

public enum RecipientStatus
{
    Active,
    Unsubscribed,
    Bounced,
    ComplainedAsSpam,
}
