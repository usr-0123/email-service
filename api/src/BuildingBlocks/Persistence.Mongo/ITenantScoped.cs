namespace EmailPlatform.BuildingBlocks.Persistence.Mongo;

public interface ITenantScoped
{
    string TenantId { get; }
}
