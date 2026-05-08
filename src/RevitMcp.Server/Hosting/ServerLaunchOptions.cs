using RevitMcp.Server.Backends;

namespace RevitMcp.Server.Hosting;

public sealed class ServerLaunchOptions
{
    public const string DefaultRemoteUrl = "http://127.0.0.1:4863/mcp";

    public BackendMode Backend { get; init; } = BackendMode.Fixture;
    public string? FixturePath { get; init; }
    public string? RemoteUrl { get; init; }
    public string? BearerToken { get; init; }

    public static ServerLaunchOptions Parse(string[] args)
    {
        var backendValue = Environment.GetEnvironmentVariable("REVIT_MCP_BACKEND");
        var fixturePath = Environment.GetEnvironmentVariable("REVIT_MCP_FIXTURE");
        var remoteUrl = Environment.GetEnvironmentVariable("REVIT_MCP_REMOTE_URL");
        var bearerToken = Environment.GetEnvironmentVariable("REVIT_MCP_BEARER_TOKEN");

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--backend" when i + 1 < args.Length:
                    backendValue = args[++i];
                    break;
                case "--fixture" when i + 1 < args.Length:
                    fixturePath = args[++i];
                    break;
                case "--remote-url" when i + 1 < args.Length:
                    remoteUrl = args[++i];
                    break;
                case "--bearer-token" when i + 1 < args.Length:
                    bearerToken = args[++i];
                    break;
            }
        }

        var backend = ParseBackend(backendValue);
        if (backend == BackendMode.Remote && string.IsNullOrWhiteSpace(remoteUrl))
        {
            remoteUrl = DefaultRemoteUrl;
        }

        return new ServerLaunchOptions
        {
            Backend = backend,
            FixturePath = fixturePath,
            RemoteUrl = remoteUrl,
            BearerToken = bearerToken
        };
    }

    public static string GetBundledFixturePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "sample-project.json");
    }

    private static BackendMode ParseBackend(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "fixture" => BackendMode.Fixture,
            "remote" => BackendMode.Remote,
            _ => throw new InvalidOperationException($"Unsupported backend mode '{value}'.")
        };
    }
}
