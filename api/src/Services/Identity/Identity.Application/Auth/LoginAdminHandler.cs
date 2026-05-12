using EmailPlatform.BuildingBlocks.Common;
using EmailPlatform.Services.Identity.Application.Abstractions;
using EmailPlatform.Services.Identity.Domain;

namespace EmailPlatform.Services.Identity.Application.Auth;

public sealed class LoginAdminHandler
{
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(7);

    private readonly IAdminUserRepository _admins;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtIssuer _jwt;
    private readonly IClock _clock;

    public LoginAdminHandler(
        IAdminUserRepository admins,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher,
        IJwtIssuer jwt,
        IClock clock)
    {
        _admins = admins;
        _refreshTokens = refreshTokens;
        _hasher = hasher;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<Result<TokenResponse>> HandleAsync(string email, string password, CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var admin = await _admins.FindByEmailAsync(normalizedEmail, ct);
        if (admin is null || !_hasher.Verify(password, admin.PasswordHash))
        {
            return Result<TokenResponse>.Failure(Error.Unauthorized("invalid credentials"));
        }

        var now = _clock.UtcNow;
        var access = _jwt.IssueAdminAccessToken(admin.Id, admin.Email, admin.Roles, now);
        var (refreshPlain, refreshHash) = RefreshTokenGenerator.Generate();

        var refreshToken = RefreshToken.Create(
            tokenHash: refreshHash,
            subjectId: admin.Id,
            tenantId: null,
            kind: RefreshTokenKind.AdminUser,
            now: now,
            lifetime: RefreshLifetime);
        await _refreshTokens.InsertAsync(refreshToken, ct);

        return Result<TokenResponse>.Success(new TokenResponse(
            AccessToken: access.Token,
            AccessTokenExpiresAt: access.ExpiresAtUtc,
            RefreshToken: refreshPlain,
            RefreshTokenExpiresAt: refreshToken.ExpiresAt));
    }
}
