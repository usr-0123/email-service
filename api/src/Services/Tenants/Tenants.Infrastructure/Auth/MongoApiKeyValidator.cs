using EmailPlatform.BuildingBlocks.Authentication;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Application.ApiKeys;

namespace EmailPlatform.Services.Tenants.Infrastructure.Auth;

internal sealed class MongoApiKeyValidator : IApiKeyValidator
{
    private readonly IApiKeyRepository _repo;

    public MongoApiKeyValidator(IApiKeyRepository repo) => _repo = repo;

    public async Task<ApiKeyValidationResult> ValidateAsync(string presentedKey, CancellationToken ct)
    {
        var hash = ApiKeyService.HashKey(presentedKey);
        var stored = await _repo.FindByHashCrossTenantAsync(hash, ct);
        if (stored is null || !stored.IsActive)
        {
            return ApiKeyValidationResult.Invalid();
        }

        _ = _repo.TouchLastUsedCrossTenantAsync(stored.Id, DateTime.UtcNow, ct);

        return ApiKeyValidationResult.Valid(
            tenantId: stored.TenantId,
            keyId: stored.Id,
            keyName: stored.Name);
    }
}
