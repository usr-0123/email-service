using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.BuildingBlocks.Observability;
using EmailPlatform.Services.Identity.Api.Endpoints;
using EmailPlatform.Services.Identity.Application;
using EmailPlatform.Services.Identity.Infrastructure;

const string ServiceName = "Identity.Api";

var builder = WebApplication.CreateBuilder(args);

builder.AddPlatformObservability(ServiceName);

builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddIdentityApplication();
builder.Services.AddMultitenancy();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseTenantResolution();

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
}));

app.MapGet("/", () => Results.Ok(new { service = ServiceName, version = "0.1.0-phase1" }));

app.MapAuthEndpoints();
app.MapAdminAuthEndpoints();
app.MapDiscoveryEndpoints();

app.Run();
