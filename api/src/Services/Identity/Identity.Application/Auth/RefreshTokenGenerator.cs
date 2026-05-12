using System.Security.Cryptography;
using System.Text;

namespace EmailPlatform.Services.Identity.Application.Auth;

internal static class RefreshTokenGenerator
{
    public static (string Plain, string Hash) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var plain = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var hash = HashToken(plain);
        return (plain, hash);
    }

    public static string HashToken(string plain)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
