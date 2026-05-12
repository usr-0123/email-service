namespace EmailPlatform.Services.Identity.Domain;

public class AdminUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static AdminUser Create(string email, string passwordHash, IEnumerable<string>? roles = null)
    {
        var now = DateTime.UtcNow;
        return new AdminUser
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Roles = roles?.ToList() ?? new List<string> { "platform_admin" },
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}
