using EmailPlatform.BuildingBlocks.Common;
using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Audiences;

public sealed class AudienceService
{
    private readonly IAudienceRepository _repo;
    private readonly ITenantContext _tenant;

    public AudienceService(IAudienceRepository repo, ITenantContext tenant)
    {
        _repo = repo;
        _tenant = tenant;
    }

    private string TenantId =>
        _tenant.CurrentTenantId
        ?? throw new InvalidOperationException("Tenant context required.");

    public async Task<Result<Audience>> CreateAsync(string name, string? description, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Audience>.Failure(Error.Validation("Name is required."));
        }

        var audience = Audience.Create(TenantId, name, description);
        await _repo.InsertAsync(audience, ct);
        return Result<Audience>.Success(audience);
    }

    public Task<IReadOnlyList<Audience>> ListAsync(CancellationToken ct)
        => _repo.ListAsync(filter: null, ct);

    public Task<Audience?> GetAsync(string id, CancellationToken ct)
        => _repo.FindByIdAsync(id, ct);

    public Task<bool> DeleteAsync(string id, CancellationToken ct)
        => _repo.DeleteAsync(id, ct);
}
