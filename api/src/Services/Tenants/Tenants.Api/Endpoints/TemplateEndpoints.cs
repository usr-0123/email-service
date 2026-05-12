using EmailPlatform.Services.Tenants.Application.Templates;

namespace EmailPlatform.Services.Tenants.Api.Endpoints;

public static class TemplateEndpoints
{
    public static IEndpointRouteBuilder MapTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/me/templates").WithTags("Templates").RequireAuthorization();

        group.MapPost("/", async (CreateTemplateRequest req, TemplateService svc, CancellationToken ct) =>
        {
            var result = await svc.CreateAsync(
                key: req.Key,
                locale: req.Locale ?? "en-US",
                subject: req.Subject,
                htmlBody: req.HtmlBody,
                textBody: req.TextBody,
                ct: ct);
            return result.IsSuccess
                ? Results.Created($"/v1/me/templates/{result.Value!.Id}", result.Value)
                : Results.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        });

        group.MapGet("/", async (TemplateService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(ct)));

        group.MapGet("/{key}/{locale}", async (string key, string locale, TemplateService svc, CancellationToken ct) =>
        {
            var t = await svc.GetByKeyLocaleAsync(key, locale, ct);
            return t is null ? Results.NotFound() : Results.Ok(t);
        });

        group.MapDelete("/{id}", async (string id, TemplateService svc, CancellationToken ct) =>
        {
            var ok = await svc.DeleteAsync(id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    public sealed record CreateTemplateRequest(
        string Key,
        string? Locale,
        string Subject,
        string HtmlBody,
        string? TextBody);
}
