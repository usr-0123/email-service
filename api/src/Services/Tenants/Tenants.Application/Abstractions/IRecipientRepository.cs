using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Abstractions;

public interface IRecipientRepository : IRepository<Recipient>
{
    Task<IReadOnlyList<Recipient>> ListByAudienceAsync(string audienceId, CancellationToken ct);
    Task InsertManyAsync(IEnumerable<Recipient> recipients, CancellationToken ct);
}
