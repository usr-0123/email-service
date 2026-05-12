using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Infrastructure.Persistence;

internal sealed class AudienceRepository : TenantScopedRepository<Audience>, IAudienceRepository
{
    public AudienceRepository(MongoDbContext db, ITenantContext tenant)
        : base(db, tenant, "audiences")
    {
    }
}
