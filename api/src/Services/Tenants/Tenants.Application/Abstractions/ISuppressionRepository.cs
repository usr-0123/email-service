using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Abstractions;

public interface ISuppressionRepository : IRepository<Suppression>
{
    Task<Suppression?> FindByEmailAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<string>> IntersectEmailsAsync(IEnumerable<string> emails, CancellationToken ct);
}
