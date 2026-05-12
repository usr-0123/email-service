namespace EmailPlatform.BuildingBlocks.Authentication;

public interface IApiKeyValidator
{
    Task<ApiKeyValidationResult> ValidateAsync(string presentedKey, CancellationToken ct);
}

public sealed record ApiKeyValidationResult(bool IsValid, string? TenantId, string? KeyId, string? KeyName)
{
    public static ApiKeyValidationResult Invalid() => new(false, null, null, null);

    public static ApiKeyValidationResult Valid(string tenantId, string keyId, string keyName)
        => new(true, tenantId, keyId, keyName);
}
