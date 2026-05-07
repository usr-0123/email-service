namespace EmailPlatform.BuildingBlocks.Multitenancy;

public interface ITenantContext
{
    string? CurrentTenantId { get; }
    bool IsAuthenticated { get; }
}
