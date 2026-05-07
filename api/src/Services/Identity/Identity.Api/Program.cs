using EmailPlatform.BuildingBlocks.Multitenancy;
using EmailPlatform.BuildingBlocks.Observability;

const string ServiceName = "Identity.Api";

var builder = WebApplication.CreateBuilder(args);

builder.AddPlatformObservability(ServiceName);

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

app.MapGet("/", () => Results.Ok(new { service = ServiceName, version = "0.1.0-phase0" }));

app.Run();
