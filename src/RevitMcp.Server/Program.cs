using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RevitMcp.Core.Logging;
using RevitMcp.Server.Backends;
using RevitMcp.Server.Hosting;
using RevitMcp.Server.Tools;

var builder = Host.CreateApplicationBuilder(args);
var launchOptions = ServerLaunchOptions.Parse(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddSingleton(launchOptions);
builder.Services.AddSingleton<IAuditLogger, NullAuditLogger>(_ => NullAuditLogger.Instance);
builder.Services.AddSingleton<CallToolResultMapper>();

builder.Services.AddSingleton<IToolExecutionBackend>(services =>
{
    return launchOptions.Backend switch
    {
        BackendMode.Fixture => FixtureBackendFactory.Create(services, launchOptions),
        BackendMode.Remote => RemoteBackendFactory.Create(services, launchOptions),
        _ => throw new InvalidOperationException($"Unsupported backend mode '{launchOptions.Backend}'.")
    };
});

builder.Services.AddSingleton<RevitMcpTools>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<RevitMcpTools>();

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RevitMcp.Server");
logger.LogInformation("Starting RevitMcp.Server with backend mode '{BackendMode}'.", launchOptions.Backend);

await host.RunAsync();
