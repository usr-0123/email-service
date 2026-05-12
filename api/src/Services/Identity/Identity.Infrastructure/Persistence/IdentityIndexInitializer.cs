using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Identity.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EmailPlatform.Services.Identity.Infrastructure.Persistence;

internal sealed class IdentityIndexInitializer : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<IdentityIndexInitializer> _logger;

    public IdentityIndexInitializer(IServiceProvider services, ILogger<IdentityIndexInitializer> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        var users = db.GetCollection<User>("users");
        await users.Indexes.CreateOneAsync(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(x => x.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_users_email" }),
            cancellationToken: ct);

        await users.Indexes.CreateOneAsync(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(x => x.TenantId).Ascending(x => x.Email),
            new CreateIndexOptions { Name = "ix_users_tenant_email" }),
            cancellationToken: ct);

        var admins = db.GetCollection<AdminUser>("adminUsers");
        await admins.Indexes.CreateOneAsync(new CreateIndexModel<AdminUser>(
            Builders<AdminUser>.IndexKeys.Ascending(x => x.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_adminUsers_email" }),
            cancellationToken: ct);

        var refreshTokens = db.GetCollection<RefreshToken>("refreshTokens");
        await refreshTokens.Indexes.CreateOneAsync(new CreateIndexModel<RefreshToken>(
            Builders<RefreshToken>.IndexKeys.Ascending(x => x.ExpiresAt),
            new CreateIndexOptions
            {
                Name = "ix_refreshTokens_expiresAt_ttl",
                ExpireAfter = TimeSpan.Zero,
            }),
            cancellationToken: ct);

        _logger.LogInformation("Identity Mongo indexes ensured.");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
