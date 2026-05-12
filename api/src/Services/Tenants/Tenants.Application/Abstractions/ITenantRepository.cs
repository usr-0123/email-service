using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> FindByIdAsync(string id, CancellationToken ct);
    Task<Tenant?> FindBySlugAsync(string slug, CancellationToken ct);
    Task InsertAsync(Tenant tenant, CancellationToken ct);
    Task<bool> ReplaceAsync(string id, Tenant tenant, CancellationToken ct);
}
