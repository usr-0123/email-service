using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Identity.Application.Abstractions;
using EmailPlatform.Services.Identity.Domain;
using MongoDB.Driver;

namespace EmailPlatform.Services.Identity.Infrastructure.Persistence;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _collection;

    public RefreshTokenRepository(MongoDbContext db)
        => _collection = db.GetCollection<RefreshToken>("refreshTokens");

    public async Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct)
        => await _collection.Find(x => x.Id == tokenHash).FirstOrDefaultAsync(ct);

    public async Task InsertAsync(RefreshToken token, CancellationToken ct)
        => await _collection.InsertOneAsync(token, cancellationToken: ct);

    public async Task<bool> RevokeAsync(string tokenHash, DateTime revokedAtUtc, CancellationToken ct)
    {
        var update = Builders<RefreshToken>.Update.Set(x => x.RevokedAt, revokedAtUtc);
        var result = await _collection.UpdateOneAsync(x => x.Id == tokenHash, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }
}
