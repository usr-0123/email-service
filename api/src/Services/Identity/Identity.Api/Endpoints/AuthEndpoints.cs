using EmailPlatform.Services.Identity.Application.Auth;

namespace EmailPlatform.Services.Identity.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest req, LoginUserHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(req.Email, req.Password, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Json(new { error = result.Error.Code, message = result.Error.Message }, statusCode: 401);
        })
        .WithName("LoginUser");

        group.MapPost("/refresh", async (RefreshRequest req, RefreshTokensHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(req.RefreshToken, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Json(new { error = result.Error.Code, message = result.Error.Message }, statusCode: 401);
        })
        .WithName("RefreshTokens");

        return app;
    }

    public sealed record LoginRequest(string Email, string Password);
    public sealed record RefreshRequest(string RefreshToken);
}
