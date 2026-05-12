namespace EmailPlatform.Services.Identity.Domain;

public class RefreshToken
{
    public string Id { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public RefreshTokenKind Kind { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive(DateTime now) => RevokedAt is null && now < ExpiresAt;

    public static RefreshToken Create(
        string tokenHash,
        string subjectId,
        string? tenantId,
        RefreshTokenKind kind,
        DateTime now,
        TimeSpan lifetime)
    {
        return new RefreshToken
        {
            Id = tokenHash,
            SubjectId = subjectId,
            TenantId = tenantId,
            Kind = kind,
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime),
        };
    }
}

public enum RefreshTokenKind
{
    TenantUser,
    AdminUser,
}
