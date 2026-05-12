using EmailPlatform.Services.Identity.Domain;

namespace EmailPlatform.Services.Identity.Application.Abstractions;

public interface IAdminUserRepository
{
    Task<AdminUser?> FindByEmailAsync(string email, CancellationToken ct);
    Task<AdminUser?> FindByIdAsync(string id, CancellationToken ct);
    Task InsertAsync(AdminUser admin, CancellationToken ct);
}
