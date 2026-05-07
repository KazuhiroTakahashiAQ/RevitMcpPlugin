using RevitMcp.Core.Config;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Logging;
using RevitMcp.Core.Server;
using RevitMcp.Core.Tools;

namespace RevitMcp.Core.Runtime;

public sealed class RevitMcpRuntime
{
    public RevitMcpRuntime(
        RevitMcpSettings settings,
        ToolRegistry toolRegistry,
        RevitApiRequestQueue queue,
        RevitExecutionService executionService,
        McpRouter router)
    {
        Settings = settings;
        ToolRegistry = toolRegistry;
        Queue = queue;
        ExecutionService = executionService;
        Router = router;
    }

    public RevitMcpSettings Settings { get; }

    public ToolRegistry ToolRegistry { get; }

    public RevitApiRequestQueue Queue { get; }

    public RevitExecutionService ExecutionService { get; }

    public McpRouter Router { get; }

    public static RevitMcpRuntime CreateDefault(
        IExternalEventDispatcher dispatcher,
        RevitMcpSettings? settings = null,
        IAuditLogger? logger = null,
        IEnumerable<IRevitMcpTool>? tools = null)
    {
        settings ??= new RevitMcpSettings();
        logger ??= NullAuditLogger.Instance;

        var registry = new ToolRegistry(tools ?? ToolRegistry.CreateDefaultTools());
        var queue = new RevitApiRequestQueue(settings.Server.MaxQueueLength);
        var executionService = new RevitExecutionService(queue, registry, settings, dispatcher, logger);
        var router = new McpRouter(registry, executionService, settings, new RequestAuthenticator(settings), logger);

        return new RevitMcpRuntime(settings, registry, queue, executionService, router);
    }
}
