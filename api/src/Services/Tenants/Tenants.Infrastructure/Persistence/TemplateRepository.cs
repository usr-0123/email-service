using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;
using MongoDB.Driver;

namespace EmailPlatform.Services.Tenants.Infrastructure.Persistence;

internal sealed class TemplateRepository : TenantScopedRepository<Template>, ITemplateRepository
{
    public TemplateRepository(MongoDbContext db, ITenantContext tenant)
        : base(db, tenant, "templates")
    {
    }

    public async Task<Template?> FindByKeyLocaleAsync(string key, string locale, CancellationToken ct)
    {
        var filter = Builders<Template>.Filter.And(
            Builders<Template>.Filter.Eq(x => x.Key, key),
            Builders<Template>.Filter.Eq(x => x.Locale, locale));
        var list = await ListAsync(filter, ct);
        return list.FirstOrDefault();
    }
}
