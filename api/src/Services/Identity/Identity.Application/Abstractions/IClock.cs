namespace EmailPlatform.Services.Identity.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
