using EmailPlatform.Services.Tenants.Application.Tenants;

namespace EmailPlatform.Services.Tenants.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/me/tenant").WithTags("Tenant").RequireAuthorization();

        group.MapGet("/", async (TenantService svc, CancellationToken ct) =>
        {
            var t = await svc.GetCurrentAsync(ct);
            return t is null
                ? Results.NotFound()
                : Results.Ok(new
                {
                    id = t.Id,
                    name = t.Name,
                    slug = t.Slug,
                    homeRegion = t.HomeRegion,
                    status = t.Status.ToString(),
                    senderConfig = new
                    {
                        mode = t.SenderConfig.Mode.ToString(),
                        sendGridSenderId = t.SenderConfig.SendGridSenderId,
                        fromEmail = t.SenderConfig.FromEmail,
                        fromName = t.SenderConfig.FromName,
                    },
                });
        });

        return app;
    }
}
