using EmailPlatform.Services.Identity.Domain;

namespace EmailPlatform.Services.Identity.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct);
    Task InsertAsync(RefreshToken token, CancellationToken ct);
    Task<bool> RevokeAsync(string tokenHash, DateTime revokedAtUtc, CancellationToken ct);
}
