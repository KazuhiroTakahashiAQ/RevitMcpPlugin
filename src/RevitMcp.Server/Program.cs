using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using RevitMcp.Core.Config;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Logging;
using RevitMcp.Core.Protocol;
using RevitMcp.Core.Runtime;
using RevitMcp.RevitAdapter.InMemory;
using System.ComponentModel;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddSingleton<IAuditLogger, NullAuditLogger>(_ => NullAuditLogger.Instance);
builder.Services.AddSingleton<ServerLaunchOptions>(_ => ServerLaunchOptions.Parse(args));
builder.Services.AddSingleton<InMemoryRevitDocumentContext>(services =>
{
    var options = services.GetRequiredService<ServerLaunchOptions>();
    return string.IsNullOrWhiteSpace(options.FixturePath)
        ? InMemorySampleProjectFactory.LoadFromFile(ServerLaunchOptions.GetBundledFixturePath())
        : InMemorySampleProjectFactory.LoadFromFile(options.FixturePath);
});
builder.Services.AddSingleton<InMemoryRevitApplicationContext>(services =>
    new(services.GetRequiredService<InMemoryRevitDocumentContext>()));
builder.Services.AddSingleton<ImmediateExternalEventDispatcher>();
builder.Services.AddSingleton(services =>
{
    var dispatcher = services.GetRequiredService<ImmediateExternalEventDispatcher>();
    var logger = services.GetRequiredService<IAuditLogger>();
    var runtime = RevitMcpRuntime.CreateDefault(dispatcher, new RevitMcpSettings(), logger);
    var application = services.GetRequiredService<InMemoryRevitApplicationContext>();
    dispatcher.Bind(ct => runtime.ExecutionService.DrainAsync(application, ct));
    return runtime;
});
builder.Services.AddSingleton<RuntimeToolExecutor>();
builder.Services.AddSingleton<RevitMcpTools>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<RevitMcpTools>();

await builder.Build().RunAsync();

[McpServerToolType]
public sealed class RevitMcpTools
{
    [McpServerTool(
        Name = "revit.document.get_info",
        Title = "Get document info",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Get information about the active Revit document.")]
    public Task<CallToolResult> GetDocumentInfo(
        [Description("Include worksharing details in the response.")] bool includeWorksharing,
        [Description("Include the document path in the response.")] bool includePath,
        RuntimeToolExecutor executor,
        CancellationToken cancellationToken)
    {
        return executor.ExecuteAsync(
            "revit.document.get_info",
            new
            {
                includeWorksharing,
                includePath
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.selection.get",
        Title = "Get selection",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Get selected elements from the active document.")]
    public Task<CallToolResult> GetSelection(
        [Description("Include element parameters in the response.")] bool includeParameters,
        [Description("Maximum number of parameters to include per element when includeParameters is true.")] int parameterLimit,
        RuntimeToolExecutor executor,
        CancellationToken cancellationToken)
    {
        return executor.ExecuteAsync(
            "revit.selection.get",
            new
            {
                includeParameters,
                parameterLimit
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.elements.find",
        Title = "Find elements",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Find elements by category, name, and parameter filters.")]
    public Task<CallToolResult> FindElements(
        [Description("Optional BuiltInCategory name. Example: OST_Walls.")] string? category,
        [Description("Optional case-insensitive substring match against element names.")] string? nameContains,
        [Description("Optional parameter name for exact string matching.")] string? parameterName,
        [Description("Optional parameter value for exact string matching.")] string? parameterValue,
        [Description("Maximum number of elements to return.")] int limit,
        RuntimeToolExecutor executor,
        CancellationToken cancellationToken)
    {
        object? parameterEquals = null;
        if (!string.IsNullOrWhiteSpace(parameterName) && !string.IsNullOrWhiteSpace(parameterValue))
        {
            parameterEquals = new
            {
                name = parameterName,
                value = parameterValue
            };
        }

        return executor.ExecuteAsync(
            "revit.elements.find",
            new
            {
                category,
                nameContains,
                parameterEquals,
                limit
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.elements.get_parameters",
        Title = "Get element parameters",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Get parameters from a Revit element.")]
    public Task<CallToolResult> GetParameters(
        [Description("Element id.")] int elementId,
        [Description("Include built-in parameters when available.")] bool includeBuiltIn,
        [Description("Include read-only parameters in the response.")] bool includeReadOnly,
        RuntimeToolExecutor executor,
        CancellationToken cancellationToken)
    {
        return executor.ExecuteAsync(
            "revit.elements.get_parameters",
            new
            {
                elementId,
                includeBuiltIn,
                includeReadOnly
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.elements.set_parameter",
        Title = "Set element parameter",
        ReadOnly = false,
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Set a parameter value on a Revit element.")]
    public Task<CallToolResult> SetParameter(
        [Description("Element id.")] int elementId,
        [Description("Parameter name.")] string parameterName,
        [Description("Parameter value as string, number, integer, or boolean.")] object value,
        RuntimeToolExecutor executor,
        CancellationToken cancellationToken)
    {
        return executor.ExecuteAsync(
            "revit.elements.set_parameter",
            new
            {
                elementId,
                parameterName,
                value
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.wall.create_line",
        Title = "Create line wall",
        ReadOnly = false,
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Create a wall on a line in the active document.")]
    public Task<CallToolResult> CreateWallLine(
        [Description("Start point X in millimeters.")] double startX,
        [Description("Start point Y in millimeters.")] double startY,
        [Description("Start point Z in millimeters.")] double startZ,
        [Description("End point X in millimeters.")] double endX,
        [Description("End point Y in millimeters.")] double endY,
        [Description("End point Z in millimeters.")] double endZ,
        [Description("Level name.")] string levelName,
        [Description("Optional wall type name.")] string? wallTypeName,
        [Description("Wall height in millimeters.")] double heightMm,
        RuntimeToolExecutor executor,
        CancellationToken cancellationToken)
    {
        return executor.ExecuteAsync(
            "revit.wall.create_line",
            new
            {
                start = new { x = startX, y = startY, z = startZ },
                end = new { x = endX, y = endY, z = endZ },
                levelName,
                wallTypeName,
                heightMm
            },
            cancellationToken);
    }
}

public sealed class RuntimeToolExecutor
{
    private readonly RevitMcpRuntime _runtime;

    public RuntimeToolExecutor(RevitMcpRuntime runtime)
    {
        _runtime = runtime;
    }

    public async Task<CallToolResult> ExecuteAsync(string toolName, object arguments, CancellationToken cancellationToken)
    {
        var jsonArguments = JsonSerializerExtensions.ToJsonObject(arguments);
        var coreResult = await _runtime.ExecutionService.EnqueueAndWaitAsync(toolName, jsonArguments, cancellationToken);
        return Map(coreResult);
    }

    private static CallToolResult Map(McpToolResult result)
    {
        var response = new CallToolResult
        {
            IsError = result.IsError,
            Content = result.Content
                .Select(content => (ContentBlock)new TextContentBlock
                {
                    Text = content.Text
                })
                .ToList()
        };

        if (result.StructuredContent is not null)
        {
            response.StructuredContent = JsonDocument.Parse(result.StructuredContent.ToJsonString()).RootElement.Clone();
        }

        return response;
    }
}

public sealed class ServerLaunchOptions
{
    public string? FixturePath { get; init; }

    public static ServerLaunchOptions Parse(string[] args)
    {
        string? fixturePath = Environment.GetEnvironmentVariable("REVIT_MCP_FIXTURE");

        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--fixture", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                fixturePath = args[i + 1];
                i++;
            }
        }

        return new ServerLaunchOptions
        {
            FixturePath = fixturePath
        };
    }

    public static string GetBundledFixturePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "sample-project.json");
    }
}
