using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace EmailPlatform.BuildingBlocks.Observability;

public static class ObservabilityHostExtensions
{
    public static WebApplicationBuilder AddPlatformObservability(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        var options = builder.Configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        var environment = builder.Environment.EnvironmentName;
        var resourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = serviceName,
            ["deployment.environment"] = environment,
        };

        builder.Host.UseSerilog((ctx, _, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("service.name", serviceName)
            .Enrich.WithProperty("deployment.environment", environment)
            .WriteTo.Console()
            .WriteTo.OpenTelemetry(o =>
            {
                o.Endpoint = options.OtlpEndpoint;
                o.Protocol = OtlpProtocol.Grpc;
                o.ResourceAttributes = resourceAttributes;
            }));

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(rb => rb
                .AddService(serviceName: serviceName)
                .AddAttributes(resourceAttributes))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint)))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint)));

        return builder;
    }
}
