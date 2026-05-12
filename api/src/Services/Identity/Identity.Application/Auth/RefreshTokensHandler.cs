using EmailPlatform.BuildingBlocks.Common;
using EmailPlatform.Services.Identity.Application.Abstractions;
using EmailPlatform.Services.Identity.Domain;

namespace EmailPlatform.Services.Identity.Application.Auth;

public sealed class RefreshTokensHandler
{
    private static readonly TimeSpan TenantRefreshLifetime = TimeSpan.FromDays(30);
    private static readonly TimeSpan AdminRefreshLifetime = TimeSpan.FromDays(7);

    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;
    private readonly IAdminUserRepository _admins;
    private readonly IJwtIssuer _jwt;
    private readonly IClock _clock;

    public RefreshTokensHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        IAdminUserRepository admins,
        IJwtIssuer jwt,
        IClock clock)
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _admins = admins;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<Result<TokenResponse>> HandleAsync(string presentedRefreshToken, CancellationToken ct)
    {
        var presentedHash = RefreshTokenGenerator.HashToken(presentedRefreshToken);

        var stored = await _refreshTokens.FindByHashAsync(presentedHash, ct);
        var now = _clock.UtcNow;

        if (stored is null || !stored.IsActive(now))
        {
            return Result<TokenResponse>.Failure(Error.Unauthorized("invalid refresh token"));
        }

        // Rotate: revoke the presented token, issue a new pair.
        await _refreshTokens.RevokeAsync(stored.Id, now, ct);

        IssuedToken access;
        TimeSpan refreshLifetime;
        string? tenantId;

        if (stored.Kind == RefreshTokenKind.TenantUser)
        {
            var user = await _users.FindByIdAsync(stored.SubjectId, ct);
            if (user is null)
            {
                return Result<TokenResponse>.Failure(Error.Unauthorized("subject not found"));
            }
            access = _jwt.IssueUserAccessToken(user.Id, user.TenantId, user.Email, user.Roles, now);
            refreshLifetime = TenantRefreshLifetime;
            tenantId = user.TenantId;
        }
        else
        {
            var admin = await _admins.FindByIdAsync(stored.SubjectId, ct);
            if (admin is null)
            {
                return Result<TokenResponse>.Failure(Error.Unauthorized("subject not found"));
            }
            access = _jwt.IssueAdminAccessToken(admin.Id, admin.Email, admin.Roles, now);
            refreshLifetime = AdminRefreshLifetime;
            tenantId = null;
        }

        var (refreshPlain, refreshHash) = RefreshTokenGenerator.Generate();
        var newRefresh = RefreshToken.Create(
            tokenHash: refreshHash,
            subjectId: stored.SubjectId,
            tenantId: tenantId,
            kind: stored.Kind,
            now: now,
            lifetime: refreshLifetime);
        await _refreshTokens.InsertAsync(newRefresh, ct);

        return Result<TokenResponse>.Success(new TokenResponse(
            AccessToken: access.Token,
            AccessTokenExpiresAt: access.ExpiresAtUtc,
            RefreshToken: refreshPlain,
            RefreshTokenExpiresAt: newRefresh.ExpiresAt));
    }
}
