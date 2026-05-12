namespace EmailPlatform.Services.Identity.Domain;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Locale { get; set; } = "en-US";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static User Create(
        string tenantId,
        string email,
        string passwordHash,
        IEnumerable<string>? roles = null,
        string locale = "en-US")
    {
        var now = DateTime.UtcNow;
        return new User
        {
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Roles = roles?.ToList() ?? new List<string> { "user" },
            Locale = locale,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}
