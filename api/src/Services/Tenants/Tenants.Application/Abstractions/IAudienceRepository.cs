using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Abstractions;

public interface IAudienceRepository : IRepository<Audience>
{
}
