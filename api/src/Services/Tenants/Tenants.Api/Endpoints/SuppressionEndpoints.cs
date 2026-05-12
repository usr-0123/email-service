using EmailPlatform.Services.Tenants.Application.Suppressions;

namespace EmailPlatform.Services.Tenants.Api.Endpoints;

public static class SuppressionEndpoints
{
    public static IEndpointRouteBuilder MapSuppressionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/me/suppressions").WithTags("Suppressions").RequireAuthorization();

        group.MapGet("/", async (SuppressionService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(ct)));

        return app;
    }
}
