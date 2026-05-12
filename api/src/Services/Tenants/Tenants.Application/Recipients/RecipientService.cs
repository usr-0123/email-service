using EmailPlatform.BuildingBlocks.Common;
using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Recipients;

public sealed record BulkCreateRecipientInput(
    string Email,
    string? Name,
    string Locale,
    IReadOnlyList<string>? AudienceIds,
    IDictionary<string, string>? CustomFields);

public sealed record BulkCreateResult(int Inserted, int Failed);

public sealed class RecipientService
{
    private readonly IRecipientRepository _repo;
    private readonly ITenantContext _tenant;

    public RecipientService(IRecipientRepository repo, ITenantContext tenant)
    {
        _repo = repo;
        _tenant = tenant;
    }

    private string TenantId =>
        _tenant.CurrentTenantId
        ?? throw new InvalidOperationException("Tenant context required.");

    public async Task<Result<BulkCreateResult>> BulkCreateAsync(
        IEnumerable<BulkCreateRecipientInput> inputs,
        CancellationToken ct)
    {
        var list = inputs.ToList();
        if (list.Count == 0)
        {
            return Result<BulkCreateResult>.Failure(Error.Validation("At least one recipient is required."));
        }

        var recipients = list.Select(x => Recipient.Create(
                tenantId: TenantId,
                email: x.Email,
                name: x.Name,
                locale: x.Locale,
                audienceIds: x.AudienceIds,
                customFields: x.CustomFields))
            .ToList();

        try
        {
            await _repo.InsertManyAsync(recipients, ct);
            return Result<BulkCreateResult>.Success(new BulkCreateResult(recipients.Count, 0));
        }
        catch (MongoDB.Driver.MongoBulkWriteException<Recipient> ex)
        {
            var inserted = recipients.Count - ex.WriteErrors.Count;
            return Result<BulkCreateResult>.Success(new BulkCreateResult(inserted, ex.WriteErrors.Count));
        }
    }

    public Task<IReadOnlyList<Recipient>> ListAsync(string? audienceId, CancellationToken ct)
        => audienceId is null
            ? _repo.ListAsync(filter: null, ct)
            : _repo.ListByAudienceAsync(audienceId, ct);

    public Task<bool> DeleteAsync(string id, CancellationToken ct)
        => _repo.DeleteAsync(id, ct);
}
