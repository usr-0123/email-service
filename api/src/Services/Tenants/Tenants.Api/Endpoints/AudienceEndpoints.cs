using EmailPlatform.Services.Tenants.Application.Audiences;

namespace EmailPlatform.Services.Tenants.Api.Endpoints;

public static class AudienceEndpoints
{
    public static IEndpointRouteBuilder MapAudienceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/me/audiences").WithTags("Audiences").RequireAuthorization();

        group.MapPost("/", async (CreateAudienceRequest req, AudienceService svc, CancellationToken ct) =>
        {
            var result = await svc.CreateAsync(req.Name, req.Description, ct);
            return result.IsSuccess
                ? Results.Created($"/v1/me/audiences/{result.Value!.Id}", result.Value)
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        });

        group.MapGet("/", async (AudienceService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(ct)));

        group.MapGet("/{id}", async (string id, AudienceService svc, CancellationToken ct) =>
        {
            var a = await svc.GetAsync(id, ct);
            return a is null ? Results.NotFound() : Results.Ok(a);
        });

        group.MapDelete("/{id}", async (string id, AudienceService svc, CancellationToken ct) =>
        {
            var ok = await svc.DeleteAsync(id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    public sealed record CreateAudienceRequest(string Name, string? Description);
}
