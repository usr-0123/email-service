using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Identity.Application.Abstractions;
using EmailPlatform.Services.Identity.Domain;
using MongoDB.Driver;

namespace EmailPlatform.Services.Identity.Infrastructure.Persistence;

internal sealed class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoDbContext db)
        => _collection = db.GetCollection<User>("users");

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct)
        => await _collection.Find(x => x.Email == email).FirstOrDefaultAsync(ct);

    public async Task<User?> FindByIdAsync(string id, CancellationToken ct)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task InsertAsync(User user, CancellationToken ct)
        => await _collection.InsertOneAsync(user, cancellationToken: ct);
}
