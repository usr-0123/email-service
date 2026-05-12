using EmailPlatform.Services.Identity.Application.Abstractions;

namespace EmailPlatform.Services.Identity.Api.Endpoints;

public static class DiscoveryEndpoints
{
    public static IEndpointRouteBuilder MapDiscoveryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/.well-known/openid-configuration", (HttpContext ctx, IJwtIssuer jwt) =>
        {
            var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            var cfg = jwt.GetOpenIdConfiguration(baseUrl);
            return Results.Ok(new
            {
                issuer = cfg.Issuer,
                jwks_uri = cfg.JwksUri,
                id_token_signing_alg_values_supported = cfg.IdTokenSigningAlgValuesSupported,
                response_types_supported = new[] { "token" },
            });
        }).WithTags("Discovery");

        app.MapGet("/.well-known/jwks.json", (IJwtIssuer jwt) =>
        {
            var jwks = jwt.GetPublicJsonWebKeySet();
            return Results.Ok(new
            {
                keys = jwks.Keys.Select(k => new
                {
                    kty = k.Kty,
                    use = k.Use,
                    kid = k.Kid,
                    alg = k.Alg,
                    n = k.N,
                    e = k.E,
                }),
            });
        }).WithTags("Discovery");

        return app;
    }
}
