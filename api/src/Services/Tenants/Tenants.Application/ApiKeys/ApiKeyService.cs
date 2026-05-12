using System.Security.Cryptography;
using System.Text;
using EmailPlatform.BuildingBlocks.Common;
using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.ApiKeys;

public sealed record CreateApiKeyResult(
    string Id,
    string Name,
    string Prefix,
    string PlaintextKey,
    DateTime CreatedAt);

public sealed record ApiKeyView(
    string Id,
    string Name,
    string Prefix,
    DateTime CreatedAt,
    DateTime? LastUsedAt,
    DateTime? RevokedAt);

public sealed class ApiKeyService
{
    private readonly IApiKeyRepository _repo;
    private readonly ITenantContext _tenant;

    public ApiKeyService(IApiKeyRepository repo, ITenantContext tenant)
    {
        _repo = repo;
        _tenant = tenant;
    }

    private string TenantId =>
        _tenant.CurrentTenantId
        ?? throw new InvalidOperationException("Tenant context required.");

    public async Task<Result<CreateApiKeyResult>> CreateAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<CreateApiKeyResult>.Failure(Error.Validation("Name is required."));
        }

        var (plain, hash, prefix) = GenerateKey();
        var key = ApiKey.Create(TenantId, name, hash, prefix);
        await _repo.InsertAsync(key, ct);

        return Result<CreateApiKeyResult>.Success(new CreateApiKeyResult(
            Id: key.Id,
            Name: key.Name,
            Prefix: key.Prefix,
            PlaintextKey: plain,
            CreatedAt: key.CreatedAt));
    }

    public async Task<IReadOnlyList<ApiKeyView>> ListAsync(CancellationToken ct)
    {
        var keys = await _repo.ListAsync(filter: null, ct);
        return keys.Select(k => new ApiKeyView(
                Id: k.Id,
                Name: k.Name,
                Prefix: k.Prefix,
                CreatedAt: k.CreatedAt,
                LastUsedAt: k.LastUsedAt,
                RevokedAt: k.RevokedAt))
            .ToList();
    }

    public Task<bool> RevokeAsync(string id, CancellationToken ct)
        => _repo.RevokeAsync(id, DateTime.UtcNow, ct);

    public static (string Plain, string Hash, string Prefix) GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var raw = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var plain = $"ek_live_{raw}";
        var hash = HashKey(plain);
        var prefix = plain[..Math.Min(12, plain.Length)];
        return (plain, hash, prefix);
    }

    public static string HashKey(string plain)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
