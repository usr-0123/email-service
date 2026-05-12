using EmailPlatform.BuildingBlocks.Common;
using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.Services.Tenants.Application.Abstractions;
using EmailPlatform.Services.Tenants.Domain;

namespace EmailPlatform.Services.Tenants.Application.Templates;

public sealed class TemplateService
{
    private readonly ITemplateRepository _repo;
    private readonly ITenantContext _tenant;

    public TemplateService(ITemplateRepository repo, ITenantContext tenant)
    {
        _repo = repo;
        _tenant = tenant;
    }

    private string TenantId =>
        _tenant.CurrentTenantId
        ?? throw new InvalidOperationException("Tenant context required.");

    public async Task<Result<Template>> CreateAsync(
        string key,
        string locale,
        string subject,
        string htmlBody,
        string? textBody,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Result<Template>.Failure(Error.Validation("Key is required."));
        }
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Result<Template>.Failure(Error.Validation("Subject is required."));
        }
        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            return Result<Template>.Failure(Error.Validation("HtmlBody is required."));
        }

        var template = Template.Create(TenantId, key, locale, subject, htmlBody, textBody);
        await _repo.InsertAsync(template, ct);
        return Result<Template>.Success(template);
    }

    public Task<IReadOnlyList<Template>> ListAsync(CancellationToken ct)
        => _repo.ListAsync(filter: null, ct);

    public Task<Template?> GetByKeyLocaleAsync(string key, string locale, CancellationToken ct)
        => _repo.FindByKeyLocaleAsync(key, locale, ct);

    public Task<bool> DeleteAsync(string id, CancellationToken ct)
        => _repo.DeleteAsync(id, ct);
}
