namespace EmailPlatform.BuildingBlocks.Contracts;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
}
