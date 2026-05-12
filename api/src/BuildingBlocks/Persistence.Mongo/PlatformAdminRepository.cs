using MongoDB.Driver;

namespace EmailPlatform.BuildingBlocks.Persistence.Mongo;

public abstract class PlatformAdminRepository<T> : IPlatformAdminRepository<T>
    where T : class
{
    private readonly IMongoCollection<T> _collection;

    protected PlatformAdminRepository(MongoDbContext context, string collectionName)
    {
        _collection = context.GetCollection<T>(collectionName);
    }

    protected IMongoCollection<T> Collection => _collection;

    public virtual async Task<T?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(FilterDefinition<T>? filter = null, CancellationToken ct = default)
    {
        filter ??= Builders<T>.Filter.Empty;
        return await _collection.Find(filter).ToListAsync(ct);
    }

    public virtual async Task InsertAsync(T entity, CancellationToken ct = default)
        => await _collection.InsertOneAsync(entity, cancellationToken: ct);

    public virtual async Task<bool> ReplaceAsync(string id, T entity, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        var result = await _collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public virtual async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        var result = await _collection.DeleteOneAsync(filter, ct);
        return result.DeletedCount > 0;
    }
}
