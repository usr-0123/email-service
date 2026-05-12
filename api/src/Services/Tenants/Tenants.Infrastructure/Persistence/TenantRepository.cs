using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;
using MongoDB.Driver;

namespace EmailPlatform.Services.Tenants.Infrastructure.Persistence;

internal sealed class TenantRepository : PlatformAdminRepository<Tenant>, ITenantRepository
{
    public TenantRepository(MongoDbContext db) : base(db, "tenants")
    {
    }

    public async Task<Tenant?> FindBySlugAsync(string slug, CancellationToken ct)
    {
        var normalized = slug.ToLowerInvariant();
        return await Collection.Find(x => x.Slug == normalized).FirstOrDefaultAsync(ct);
    }
}
