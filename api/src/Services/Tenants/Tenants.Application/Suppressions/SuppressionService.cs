using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Suppressions;

public sealed class SuppressionService
{
    private readonly ISuppressionRepository _repo;
    private readonly ITenantContext _tenant;

    public SuppressionService(ISuppressionRepository repo, ITenantContext tenant)
    {
        _repo = repo;
        _tenant = tenant;
    }

    public Task<IReadOnlyList<Suppression>> ListAsync(CancellationToken ct)
        => _repo.ListAsync(filter: null, ct);

    public Task<IReadOnlyList<string>> IntersectEmailsAsync(IEnumerable<string> emails, CancellationToken ct)
        => _repo.IntersectEmailsAsync(emails, ct);
}
