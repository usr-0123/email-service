using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Tenants.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EmailPlatform.Services.Tenants.Infrastructure.Persistence;

internal sealed class TenantsIndexInitializer : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TenantsIndexInitializer> _logger;

    public TenantsIndexInitializer(IServiceProvider services, ILogger<TenantsIndexInitializer> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        await db.GetCollection<Tenant>("tenants").Indexes.CreateOneAsync(
            new CreateIndexModel<Tenant>(
                Builders<Tenant>.IndexKeys.Ascending(x => x.Slug),
                new CreateIndexOptions { Unique = true, Name = "ux_tenants_slug" }),
            cancellationToken: ct);

        var apiKeys = db.GetCollection<ApiKey>("apiKeys");
        await apiKeys.Indexes.CreateOneAsync(
            new CreateIndexModel<ApiKey>(
                Builders<ApiKey>.IndexKeys.Ascending(x => x.KeyHash),
                new CreateIndexOptions { Unique = true, Name = "ux_apiKeys_keyHash" }),
            cancellationToken: ct);
        await apiKeys.Indexes.CreateOneAsync(
            new CreateIndexModel<ApiKey>(
                Builders<ApiKey>.IndexKeys.Ascending(x => x.TenantId),
                new CreateIndexOptions { Name = "ix_apiKeys_tenantId" }),
            cancellationToken: ct);

        await db.GetCollection<Audience>("audiences").Indexes.CreateOneAsync(
            new CreateIndexModel<Audience>(
                Builders<Audience>.IndexKeys.Ascending(x => x.TenantId).Ascending(x => x.Name),
                new CreateIndexOptions { Name = "ix_audiences_tenant_name" }),
            cancellationToken: ct);

        var recipients = db.GetCollection<Recipient>("recipients");
        await recipients.Indexes.CreateOneAsync(
            new CreateIndexModel<Recipient>(
                Builders<Recipient>.IndexKeys.Ascending(x => x.TenantId).Ascending(x => x.Email),
                new CreateIndexOptions { Unique = true, Name = "ux_recipients_tenant_email" }),
            cancellationToken: ct);
        await recipients.Indexes.CreateOneAsync(
            new CreateIndexModel<Recipient>(
                Builders<Recipient>.IndexKeys.Ascending(x => x.TenantId).Ascending(x => x.AudienceIds),
                new CreateIndexOptions { Name = "ix_recipients_tenant_audiences" }),
            cancellationToken: ct);

        await db.GetCollection<Template>("templates").Indexes.CreateOneAsync(
            new CreateIndexModel<Template>(
                Builders<Template>.IndexKeys
                    .Ascending(x => x.TenantId)
                    .Ascending(x => x.Key)
                    .Ascending(x => x.Locale),
                new CreateIndexOptions { Unique = true, Name = "ux_templates_tenant_key_locale" }),
            cancellationToken: ct);

        await db.GetCollection<Suppression>("suppressions").Indexes.CreateOneAsync(
            new CreateIndexModel<Suppression>(
                Builders<Suppression>.IndexKeys.Ascending(x => x.TenantId).Ascending(x => x.Email),
                new CreateIndexOptions { Unique = true, Name = "ux_suppressions_tenant_email" }),
            cancellationToken: ct);

        _logger.LogInformation("Tenants Mongo indexes ensured.");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
