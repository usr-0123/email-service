using EmailPlatform.Services.Identity.Application.Abstractions;

namespace EmailPlatform.Services.Identity.Infrastructure.Auth;

internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
