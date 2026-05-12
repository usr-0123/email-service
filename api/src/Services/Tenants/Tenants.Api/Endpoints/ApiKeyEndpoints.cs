using EmailPlatform.Services.Tenants.Application.ApiKeys;

namespace EmailPlatform.Services.Tenants.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static IEndpointRouteBuilder MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/me/api-keys").WithTags("ApiKeys").RequireAuthorization();

        group.MapPost("/", async (CreateApiKeyRequest req, ApiKeyService svc, CancellationToken ct) =>
        {
            var result = await svc.CreateAsync(req.Name, ct);
            return result.IsSuccess
                ? Results.Created($"/v1/me/api-keys/{result.Value!.Id}", result.Value)
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        });

        group.MapGet("/", async (ApiKeyService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(ct)));

        group.MapDelete("/{id}", async (string id, ApiKeyService svc, CancellationToken ct) =>
        {
            var ok = await svc.RevokeAsync(id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    public sealed record CreateApiKeyRequest(string Name);
}
