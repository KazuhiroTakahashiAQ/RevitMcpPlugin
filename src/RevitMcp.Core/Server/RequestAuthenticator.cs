using System.Security.Cryptography;
using System.Text;
using RevitMcp.Core.Config;

namespace RevitMcp.Core.Server;

public sealed class RequestAuthenticator
{
    private readonly RevitMcpSettings _settings;

    public RequestAuthenticator(RevitMcpSettings settings)
    {
        _settings = settings;
    }

    public bool IsOriginAllowed(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return true;
        }

        return _settings.Server.AllowedOrigins.Any(allowed => string.Equals(allowed, origin, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsAuthorized(string? authorization)
    {
        if (!_settings.Server.RequireAuthToken)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(authorization))
        {
            return false;
        }

        const string prefix = "Bearer ";
        if (!authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var providedToken = authorization[prefix.Length..];
        var expectedBytes = Encoding.UTF8.GetBytes(_settings.Server.AuthToken);
        var providedBytes = Encoding.UTF8.GetBytes(providedToken);

        return expectedBytes.Length == providedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
