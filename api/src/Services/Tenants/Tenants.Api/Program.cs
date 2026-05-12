using EmailPlatform.BuildingBlocks.Authentication;
using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.BuildingBlocks.Observability;
using EmailPlatform.Services.Tenants.Api.Endpoints;
using EmailPlatform.Services.Tenants.Application;
using EmailPlatform.Services.Tenants.Infrastructure;

const string ServiceName = "Tenants.Api";

var builder = WebApplication.CreateBuilder(args);

builder.AddPlatformObservability(ServiceName);

builder.Services.AddTenantsInfrastructure(builder.Configuration);
builder.Services.AddTenantsApplication();
builder.Services.AddMultitenancy();
builder.Services.AddPlatformAuthentication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseTenantResolution();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = ServiceName,
    timestamp = DateTime.UtcNow,
})).AllowAnonymous();

app.MapGet("/", () => Results.Ok(new { service = ServiceName, version = "0.1.0-phase1" }))
    .AllowAnonymous();

app.MapTenantEndpoints();
app.MapApiKeyEndpoints();
app.MapAudienceEndpoints();
app.MapRecipientEndpoints();
app.MapTemplateEndpoints();
app.MapSuppressionEndpoints();

app.Run();
