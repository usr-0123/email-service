using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Tenants;

public sealed class TenantService
{
    private readonly ITenantRepository _repo;
    private readonly ITenantContext _tenant;

    public TenantService(ITenantRepository repo, ITenantContext tenant)
    {
        _repo = repo;
        _tenant = tenant;
    }

    private string TenantId =>
        _tenant.CurrentTenantId
        ?? throw new InvalidOperationException("Tenant context required.");

    public Task<Tenant?> GetCurrentAsync(CancellationToken ct)
        => _repo.FindByIdAsync(TenantId, ct);
}
