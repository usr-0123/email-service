using EmailPlatform.BuildingBlocks.Multitenancy;
using MongoDB.Driver;

namespace EmailPlatform.BuildingBlocks.Persistence.Mongo;

public abstract class TenantScopedRepository<T> : IRepository<T>
    where T : class, ITenantScoped
{
    private readonly IMongoCollection<T> _collection;
    private readonly ITenantContext _tenantContext;

    protected TenantScopedRepository(MongoDbContext context, ITenantContext tenantContext, string collectionName)
    {
        _collection = context.GetCollection<T>(collectionName);
        _tenantContext = tenantContext;
    }

    protected IMongoCollection<T> Collection => _collection;

    protected string CurrentTenantId =>
        _tenantContext.CurrentTenantId
        ?? throw new InvalidOperationException(
            "Tenant-scoped operation attempted without an authenticated tenant context.");

    protected FilterDefinition<T> TenantFilter()
        => Builders<T>.Filter.Eq(x => x.TenantId, CurrentTenantId);

    public virtual async Task<T?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        var filter = TenantFilter() & Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(FilterDefinition<T>? filter = null, CancellationToken ct = default)
    {
        var combined = filter is null ? TenantFilter() : TenantFilter() & filter;
        return await _collection.Find(combined).ToListAsync(ct);
    }

    public virtual async Task InsertAsync(T entity, CancellationToken ct = default)
    {
        if (!string.Equals(entity.TenantId, CurrentTenantId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Entity TenantId '{entity.TenantId}' does not match current tenant context '{CurrentTenantId}'.");
        }
        await _collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    public virtual async Task<bool> ReplaceAsync(string id, T entity, CancellationToken ct = default)
    {
        if (!string.Equals(entity.TenantId, CurrentTenantId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Entity TenantId '{entity.TenantId}' does not match current tenant context '{CurrentTenantId}'.");
        }
        var filter = TenantFilter() & Builders<T>.Filter.Eq("_id", id);
        var result = await _collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public virtual async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var filter = TenantFilter() & Builders<T>.Filter.Eq("_id", id);
        var result = await _collection.DeleteOneAsync(filter, ct);
        return result.DeletedCount > 0;
    }
}
