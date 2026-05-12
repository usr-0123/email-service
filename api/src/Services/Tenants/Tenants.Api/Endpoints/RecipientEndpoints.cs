using EmailPlatform.Services.Tenants.Application.Recipients;

namespace EmailPlatform.Services.Tenants.Api.Endpoints;

public static class RecipientEndpoints
{
    public static IEndpointRouteBuilder MapRecipientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/me/recipients").WithTags("Recipients").RequireAuthorization();

        group.MapPost("/bulk", async (BulkCreateRequest req, RecipientService svc, CancellationToken ct) =>
        {
            var inputs = req.Recipients.Select(r => new BulkCreateRecipientInput(
                Email: r.Email,
                Name: r.Name,
                Locale: r.Locale ?? "en-US",
                AudienceIds: r.AudienceIds,
                CustomFields: r.CustomFields));

            var result = await svc.BulkCreateAsync(inputs, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        });

        group.MapGet("/", async (string? audienceId, RecipientService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(audienceId, ct)));

        group.MapDelete("/{id}", async (string id, RecipientService svc, CancellationToken ct) =>
        {
            var ok = await svc.DeleteAsync(id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    public sealed record BulkCreateRequest(IReadOnlyList<RecipientInput> Recipients);

    public sealed record RecipientInput(
        string Email,
        string? Name,
        string? Locale,
        IReadOnlyList<string>? AudienceIds,
        Dictionary<string, string>? CustomFields);
}
