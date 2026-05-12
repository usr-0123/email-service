using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Abstractions;

public interface ITemplateRepository : IRepository<Template>
{
    Task<Template?> FindByKeyLocaleAsync(string key, string locale, CancellationToken ct);
}
