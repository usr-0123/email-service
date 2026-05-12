using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;
using MongoDB.Driver;

namespace EmailPlatform.Services.Tenants.Infrastructure.Persistence;

internal sealed class ApiKeyRepository : TenantScopedRepository<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(MongoDbContext db, ITenantContext tenant)
        : base(db, tenant, "apiKeys")
    {
    }

    public async Task<ApiKey?> FindByHashCrossTenantAsync(string keyHash, CancellationToken ct)
        => await Collection.Find(x => x.KeyHash == keyHash).FirstOrDefaultAsync(ct);

    public async Task<bool> TouchLastUsedCrossTenantAsync(string id, DateTime utcNow, CancellationToken ct)
    {
        var update = Builders<ApiKey>.Update.Set(x => x.LastUsedAt, utcNow);
        var result = await Collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> RevokeAsync(string id, DateTime utcNow, CancellationToken ct)
    {
        var filter = TenantFilter() & Builders<ApiKey>.Filter.Eq(x => x.Id, id);
        var update = Builders<ApiKey>.Update.Set(x => x.RevokedAt, utcNow);
        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }
}
