namespace EmailPlatform.BuildingBlocks.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
}
