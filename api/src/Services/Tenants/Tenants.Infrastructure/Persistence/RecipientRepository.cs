using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;
using MongoDB.Driver;

namespace EmailPlatform.Services.Tenants.Infrastructure.Persistence;

internal sealed class RecipientRepository : TenantScopedRepository<Recipient>, IRecipientRepository
{
    public RecipientRepository(MongoDbContext db, ITenantContext tenant)
        : base(db, tenant, "recipients")
    {
    }

    public async Task<IReadOnlyList<Recipient>> ListByAudienceAsync(string audienceId, CancellationToken ct)
    {
        var filter = Builders<Recipient>.Filter.AnyEq(x => x.AudienceIds, audienceId);
        return await ListAsync(filter, ct);
    }

    public async Task InsertManyAsync(IEnumerable<Recipient> recipients, CancellationToken ct)
    {
        var list = recipients.ToList();
        var currentTenant = CurrentTenantId;
        foreach (var r in list)
        {
            if (!string.Equals(r.TenantId, currentTenant, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Recipient TenantId '{r.TenantId}' does not match current tenant '{currentTenant}'.");
            }
        }
        await Collection.InsertManyAsync(list, new InsertManyOptions { IsOrdered = false }, ct);
    }
}
