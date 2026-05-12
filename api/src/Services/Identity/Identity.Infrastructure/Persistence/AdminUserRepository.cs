using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Identity.Application.Abstractions;
using EmailPlatform.Services.Identity.Domain;
using MongoDB.Driver;

namespace EmailPlatform.Services.Identity.Infrastructure.Persistence;

internal sealed class AdminUserRepository : IAdminUserRepository
{
    private readonly IMongoCollection<AdminUser> _collection;

    public AdminUserRepository(MongoDbContext db)
        => _collection = db.GetCollection<AdminUser>("adminUsers");

    public async Task<AdminUser?> FindByEmailAsync(string email, CancellationToken ct)
        => await _collection.Find(x => x.Email == email).FirstOrDefaultAsync(ct);

    public async Task<AdminUser?> FindByIdAsync(string id, CancellationToken ct)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task InsertAsync(AdminUser admin, CancellationToken ct)
        => await _collection.InsertOneAsync(admin, cancellationToken: ct);
}
