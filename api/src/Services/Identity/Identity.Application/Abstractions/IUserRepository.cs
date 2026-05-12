using EmailPlatform.Services.Identity.Domain;

namespace EmailPlatform.Services.Identity.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct);
    Task<User?> FindByIdAsync(string id, CancellationToken ct);
    Task InsertAsync(User user, CancellationToken ct);
}
