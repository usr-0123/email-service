namespace EmailPlatform.Services.Tenants.Domain;

public class SenderConfig
{
    public SenderMode Mode { get; set; } = SenderMode.Shared;
    public string? SendGridSenderId { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }

    public static SenderConfig UseShared() => new() { Mode = SenderMode.Shared };

    public static SenderConfig UseCustom(string sendGridSenderId, string fromEmail, string fromName)
        => new()
        {
            Mode = SenderMode.Custom,
            SendGridSenderId = sendGridSenderId,
            FromEmail = fromEmail,
            FromName = fromName,
        };
}

public enum SenderMode
{
    Shared,
    Custom,
}
