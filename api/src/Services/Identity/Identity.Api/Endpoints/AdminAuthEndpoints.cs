using EmailPlatform.Services.Identity.Application.Auth;

namespace EmailPlatform.Services.Identity.Api.Endpoints;

public static class AdminAuthEndpoints
{
    public static IEndpointRouteBuilder MapAdminAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/admin/auth").WithTags("AdminAuth");

        group.MapPost("/login", async (AdminLoginRequest req, LoginAdminHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(req.Email, req.Password, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Json(new { error = result.Error.Code, message = result.Error.Message }, statusCode: 401);
        })
        .WithName("LoginAdmin");

        return app;
    }

    public sealed record AdminLoginRequest(string Email, string Password);
}
