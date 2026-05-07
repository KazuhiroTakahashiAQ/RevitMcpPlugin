namespace RevitMcp.Core.Config;

public sealed class RevitMcpSettings
{
    public ServerSettings Server { get; init; } = new();
    public ToolSettings Tools { get; init; } = new();
    public LoggingSettings Logging { get; init; } = new();

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (!string.Equals(Server.Host, "127.0.0.1", StringComparison.Ordinal))
        {
            errors.Add("Server host must be 127.0.0.1.");
        }

        if (Server.Port is < 1024 or > 65535)
        {
            errors.Add("Server port must be between 1024 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(Server.Endpoint) || !Server.Endpoint.StartsWith("/", StringComparison.Ordinal))
        {
            errors.Add("Server endpoint must start with '/'.");
        }

        if (Server.RequestTimeoutSeconds is < 5 or > 120)
        {
            errors.Add("Request timeout must be between 5 and 120 seconds.");
        }

        if (Server.MaxQueueLength is < 1 or > 1000)
        {
            errors.Add("Max queue length must be between 1 and 1000.");
        }

        return errors;
    }
}

public sealed class ServerSettings
{
    public bool EnabledOnStartup { get; init; }
    public string Host { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 4863;
    public string Endpoint { get; init; } = "/mcp";
    public bool RequireAuthToken { get; init; }
    public string AuthToken { get; init; } = string.Empty;
    public IReadOnlyList<string> AllowedOrigins { get; init; } = new[] { "http://localhost", "http://127.0.0.1" };
    public int RequestTimeoutSeconds { get; init; } = 30;
    public int MaxQueueLength { get; init; } = 100;
    public int MaxBatchSize { get; init; } = 10;
    public int MaxExecutionSliceMilliseconds { get; init; } = 100;
}

public sealed class ToolSettings
{
    public bool EnableWriteTools { get; init; } = true;
    public bool EnableDestructiveTools { get; init; }
    public bool EnableScriptExecution { get; init; }
    public int MaxFindElementsLimit { get; init; } = 1000;
}

public sealed class LoggingSettings
{
    public string Level { get; init; } = "Info";
    public int RetentionDays { get; init; } = 14;
}
