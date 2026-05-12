using EmailPlatform.BuildingBlocks.Persistence.Mongo;

namespace EmailPlatform.Services.Tenants.Domain;

public class Template : ITenantScoped
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Locale { get; set; } = "en-US";
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static Template Create(
        string tenantId,
        string key,
        string locale,
        string subject,
        string htmlBody,
        string? textBody = null)
    {
        var now = DateTime.UtcNow;
        return new Template
        {
            TenantId = tenantId,
            Key = key,
            Locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale,
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}
