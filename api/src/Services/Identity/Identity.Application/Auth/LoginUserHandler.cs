using EmailPlatform.BuildingBlocks.Common;
using EmailPlatform.Services.Identity.Application.Abstractions;
using EmailPlatform.Services.Identity.Domain;

namespace EmailPlatform.Services.Identity.Application.Auth;

public sealed class LoginUserHandler
{
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(30);

    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtIssuer _jwt;
    private readonly IClock _clock;

    public LoginUserHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher,
        IJwtIssuer jwt,
        IClock clock)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _hasher = hasher;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<Result<TokenResponse>> HandleAsync(string email, string password, CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (user is null || !_hasher.Verify(password, user.PasswordHash))
        {
            return Result<TokenResponse>.Failure(Error.Unauthorized("invalid credentials"));
        }

        var now = _clock.UtcNow;
        var access = _jwt.IssueUserAccessToken(user.Id, user.TenantId, user.Email, user.Roles, now);
        var (refreshPlain, refreshHash) = RefreshTokenGenerator.Generate();

        var refreshToken = RefreshToken.Create(
            tokenHash: refreshHash,
            subjectId: user.Id,
            tenantId: user.TenantId,
            kind: RefreshTokenKind.TenantUser,
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
