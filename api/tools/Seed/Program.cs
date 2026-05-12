using EmailPlatform.BuildingBlocks.Persistence.Mongo;
using EmailPlatform.Services.Identity.Domain;
using EmailPlatform.Services.Tenants.Application.ApiKeys;
using EmailPlatform.Services.Tenants.Domain;
using MongoDB.Driver;

const string TenantSlug = "acme";
const string TenantName = "Acme Corp";
const string UserEmail = "admin@acme.test";
const string UserPassword = "P@ssword1!";
const string AdminEmail = "platform@emailplatform.test";
const string AdminPassword = "P@ssword1!";
const string ApiKeyName = "Default API Key";

var mongoConn = Environment.GetEnvironmentVariable("MONGO_CONN")
    ?? "mongodb://root:changeme@localhost:27017/?authSource=admin";

MongoConventions.RegisterOnce();

var client = new MongoClient(mongoConn);
var identityDb = client.GetDatabase("emailplatform_identity");
var tenantsDb = client.GetDatabase("emailplatform_tenants");

Console.WriteLine($"Seeding against {client.Settings.Server}");

// 1. Tenant — idempotent (drop existing seed by slug)
var tenants = tenantsDb.GetCollection<Tenant>("tenants");
var existingTenant = await tenants.Find(t => t.Slug == TenantSlug).FirstOrDefaultAsync();
if (existingTenant is not null)
{
    Console.WriteLine($"Existing tenant '{TenantSlug}' found ({existingTenant.Id}); wiping its records.");
    await CleanupTenantData(tenantsDb, identityDb, existingTenant.Id);
    await tenants.DeleteOneAsync(t => t.Id == existingTenant.Id);
}

var tenant = Tenant.Create(TenantName, TenantSlug, homeRegion: "ke-1");
await tenants.InsertOneAsync(tenant);
Console.WriteLine($"Tenant created: {tenant.Id} ({tenant.Slug})");

// 2. Tenant user
var users = identityDb.GetCollection<User>("users");
await users.DeleteManyAsync(u => u.Email == UserEmail);
var user = User.Create(tenant.Id, UserEmail, BCrypt.Net.BCrypt.HashPassword(UserPassword, 12));
await users.InsertOneAsync(user);
Console.WriteLine($"Tenant user created: {user.Id} ({user.Email}) password={UserPassword}");

// 3. Admin user
var admins = identityDb.GetCollection<AdminUser>("adminUsers");
await admins.DeleteManyAsync(a => a.Email == AdminEmail);
var admin = AdminUser.Create(AdminEmail, BCrypt.Net.BCrypt.HashPassword(AdminPassword, 12));
await admins.InsertOneAsync(admin);
Console.WriteLine($"Admin user created: {admin.Id} ({admin.Email}) password={AdminPassword}");

// 4. API key (plaintext shown ONCE)
var (apiKeyPlain, apiKeyHash, apiKeyPrefix) = ApiKeyService.GenerateKey();
var apiKey = ApiKey.Create(tenant.Id, ApiKeyName, apiKeyHash, apiKeyPrefix);
await tenantsDb.GetCollection<ApiKey>("apiKeys").InsertOneAsync(apiKey);
Console.WriteLine($"API key created: {apiKey.Id}");
Console.WriteLine($"  >>> Plaintext (save this, shown ONCE): {apiKeyPlain}");

// 5. Audience
var audience = Audience.Create(tenant.Id, "Newsletter Subscribers", "Default seeded audience");
await tenantsDb.GetCollection<Audience>("audiences").InsertOneAsync(audience);
Console.WriteLine($"Audience created: {audience.Id} ({audience.Name})");

// 6. Recipients
var recipients = new[]
{
    Recipient.Create(tenant.Id, "alice@example.test", "Alice", "en-US", new[] { audience.Id }, null),
    Recipient.Create(tenant.Id, "bob@example.test",   "Bob",   "en-US", new[] { audience.Id }, null),
    Recipient.Create(tenant.Id, "carol@example.test", "Carol", "fr-FR", new[] { audience.Id }, null),
};
await tenantsDb.GetCollection<Recipient>("recipients").InsertManyAsync(recipients);
Console.WriteLine($"Recipients created: {recipients.Length}");

// 7. Template (en-US)
var template = Template.Create(
    tenantId: tenant.Id,
    key: "welcome",
    locale: "en-US",
    subject: "Welcome to {{ company }}!",
    htmlBody: "<h1>Hi {{ name }},</h1><p>Welcome to {{ company }}. Your account is ready.</p>",
    textBody: "Hi {{ name }}, welcome to {{ company }}. Your account is ready.");
await tenantsDb.GetCollection<Template>("templates").InsertOneAsync(template);
Console.WriteLine($"Template created: {template.Id} ({template.Key}:{template.Locale})");

Console.WriteLine();
Console.WriteLine("Seed complete. Summary:");
Console.WriteLine($"  Tenant id        : {tenant.Id}");
Console.WriteLine($"  Tenant slug      : {tenant.Slug}");
Console.WriteLine($"  Tenant user      : {user.Email} / {UserPassword}");
Console.WriteLine($"  Admin user       : {admin.Email} / {AdminPassword}");
Console.WriteLine($"  Audience id      : {audience.Id}");
Console.WriteLine($"  Template key     : {template.Key}:{template.Locale}");
Console.WriteLine($"  API key (plain)  : {apiKeyPlain}");

static async Task CleanupTenantData(IMongoDatabase tenantsDb, IMongoDatabase identityDb, string tenantId)
{
    var tenantIdFilter = Builders<ApiKey>.Filter.Eq(x => x.TenantId, tenantId);
    await tenantsDb.GetCollection<ApiKey>("apiKeys").DeleteManyAsync(tenantIdFilter);

    await tenantsDb.GetCollection<Audience>("audiences")
        .DeleteManyAsync(Builders<Audience>.Filter.Eq(x => x.TenantId, tenantId));
    await tenantsDb.GetCollection<Recipient>("recipients")
        .DeleteManyAsync(Builders<Recipient>.Filter.Eq(x => x.TenantId, tenantId));
    await tenantsDb.GetCollection<Template>("templates")
        .DeleteManyAsync(Builders<Template>.Filter.Eq(x => x.TenantId, tenantId));
    await tenantsDb.GetCollection<Suppression>("suppressions")
        .DeleteManyAsync(Builders<Suppression>.Filter.Eq(x => x.TenantId, tenantId));

    await identityDb.GetCollection<User>("users")
        .DeleteManyAsync(Builders<User>.Filter.Eq(x => x.TenantId, tenantId));
}
