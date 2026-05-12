using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Abstractions;

public interface IApiKeyRepository : IRepository<ApiKey>
{
    Task<ApiKey?> FindByHashCrossTenantAsync(string keyHash, CancellationToken ct);
    Task<bool> TouchLastUsedCrossTenantAsync(string id, DateTime utcNow, CancellationToken ct);
    Task<bool> RevokeAsync(string id, DateTime utcNow, CancellationToken ct);
}
