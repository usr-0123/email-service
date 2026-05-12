using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;
using MongoDB.Driver;

namespace EmailPlatform.Services.Tenants.Infrastructure.Persistence;

internal sealed class SuppressionRepository : TenantScopedRepository<Suppression>, ISuppressionRepository
{
    public SuppressionRepository(MongoDbContext db, ITenantContext tenant)
        : base(db, tenant, "suppressions")
    {
    }

    public async Task<Suppression?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var filter = Builders<Suppression>.Filter.Eq(x => x.Email, normalized);
        var list = await ListAsync(filter, ct);
        return list.FirstOrDefault();
    }

    public async Task<IReadOnlyList<string>> IntersectEmailsAsync(IEnumerable<string> emails, CancellationToken ct)
    {
        var normalized = emails
            .Select(e => e.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
        {
            return Array.Empty<string>();
        }

        var filter = Builders<Suppression>.Filter.In(x => x.Email, normalized);
        var list = await ListAsync(filter, ct);
        return list.Select(s => s.Email).ToList();
    }
}
