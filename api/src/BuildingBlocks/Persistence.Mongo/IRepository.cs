using MongoDB.Driver;

namespace EmailPlatform.BuildingBlocks.Persistence.Mongo;

public interface IRepository<T> where T : class
{
    Task<T?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(FilterDefinition<T>? filter = null, CancellationToken ct = default);
    Task InsertAsync(T entity, CancellationToken ct = default);
    Task<bool> ReplaceAsync(string id, T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

public interface IPlatformAdminRepository<T> : IRepository<T> where T : class
{
}
