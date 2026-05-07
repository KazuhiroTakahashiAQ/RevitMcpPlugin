using System.Text.Json;
using System.Text.Json.Nodes;
using RevitMcp.Core.Config;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Logging;
using RevitMcp.Core.Protocol;
using RevitMcp.Core.Tools;

namespace RevitMcp.Core.Server;

public sealed class McpRouter
{
    private readonly ToolRegistry _toolRegistry;
    private readonly RevitExecutionService _executionService;
    private readonly RevitMcpSettings _settings;
    private readonly RequestAuthenticator _authenticator;
    private readonly IAuditLogger _logger;

    public McpRouter(
        ToolRegistry toolRegistry,
        RevitExecutionService executionService,
        RevitMcpSettings settings,
        RequestAuthenticator authenticator,
        IAuditLogger logger)
    {
        _toolRegistry = toolRegistry;
        _executionService = executionService;
        _settings = settings;
        _authenticator = authenticator;
        _logger = logger;
    }

    public async Task<RouterResponse> RouteAsync(string body, HttpRequestContext requestContext, CancellationToken cancellationToken = default)
    {
        if (!_authenticator.IsOriginAllowed(requestContext.Origin))
        {
            return RouterResponse.HttpOnly(403);
        }

        if (!_authenticator.IsAuthorized(requestContext.Authorization))
        {
            return RouterResponse.HttpOnly(401);
        }

        JsonNode? requestNode;

        try
        {
            requestNode = JsonNode.Parse(body);
        }
        catch (JsonException)
        {
            return RouterResponse.HttpOnly(400);
        }

        if (requestNode is not JsonObject requestObject)
        {
            return RouterResponse.Json(200, JsonRpcResponseFactory.Error(null, -32600, "Invalid Request"));
        }

        var id = requestObject["id"]?.DeepClone();
        var method = requestObject["method"]?.GetValue<string>();
        var jsonrpc = requestObject["jsonrpc"]?.GetValue<string>();

        if (!string.Equals(jsonrpc, "2.0", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(method))
        {
            return RouterResponse.Json(200, JsonRpcResponseFactory.Error(id, -32600, "Invalid Request"));
        }

        return method switch
        {
            "initialize" => RouterResponse.Json(200, BuildInitializeResponse(id)),
            "tools/list" => RouterResponse.Json(200, BuildToolsListResponse(id)),
            "tools/call" => RouterResponse.Json(200, await BuildToolCallResponseAsync(id, requestObject["params"] as JsonObject, requestContext, cancellationToken).ConfigureAwait(false)),
            _ => RouterResponse.Json(200, JsonRpcResponseFactory.Error(id, -32601, "Method not found"))
        };
    }

    private JsonObject BuildInitializeResponse(JsonNode? id)
    {
        return JsonRpcResponseFactory.Result(id, new
        {
            protocolVersion = "2025-06-18",
            capabilities = new
            {
                tools = new { }
            },
            serverInfo = new
            {
                name = "revit-mcp",
                version = "0.1.0"
            }
        });
    }

    private JsonObject BuildToolsListResponse(JsonNode? id)
    {
        var tools = _toolRegistry.List()
            .Where(IsToolEnabled)
            .Select(tool => new
            {
                name = tool.Name,
                description = tool.Description,
                inputSchema = tool.InputSchema,
                annotations = new
                {
                    title = tool.Annotations.Title,
                    readOnlyHint = tool.Annotations.ReadOnlyHint
                }
            });

        return JsonRpcResponseFactory.Result(id, new { tools });
    }

    private async Task<JsonObject> BuildToolCallResponseAsync(
        JsonNode? id,
        JsonObject? parameters,
        HttpRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        if (parameters is null)
        {
            return JsonRpcResponseFactory.Error(id, -32602, "Invalid params");
        }

        var toolName = parameters["name"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return JsonRpcResponseFactory.Error(id, -32602, "Invalid params");
        }

        if (!_toolRegistry.TryGet(toolName, out var tool))
        {
            return JsonRpcResponseFactory.Result(id, McpToolResult.Error($"Tool '{toolName}' is not registered."));
        }

        if (!IsToolEnabled(tool))
        {
            return JsonRpcResponseFactory.Error(id, -32002, "Tool disabled by policy");
        }

        var arguments = parameters["arguments"] as JsonObject ?? new JsonObject();

        _logger.Log(new AuditLogEntry(
            Timestamp: DateTimeOffset.UtcNow,
            EventType: "tools/call.received",
            Method: "tools/call",
            ToolName: toolName,
            ClientName: requestContext.ClientName));

        try
        {
            var result = await _executionService.EnqueueAndWaitAsync(toolName, arguments, cancellationToken).ConfigureAwait(false);
            return JsonRpcResponseFactory.Result(id, result);
        }
        catch (TimeoutException)
        {
            return JsonRpcResponseFactory.Error(id, -32001, "Tool execution timeout");
        }
        catch (InvalidOperationException ex)
        {
            return JsonRpcResponseFactory.Error(id, -32000, ex.Message);
        }
    }

    private bool IsToolEnabled(IRevitMcpTool tool)
    {
        return tool.AccessLevel switch
        {
            ToolAccessLevel.Read => true,
            ToolAccessLevel.Modify => _settings.Tools.EnableWriteTools,
            ToolAccessLevel.Destructive => _settings.Tools.EnableDestructiveTools,
            ToolAccessLevel.Dangerous => _settings.Tools.EnableScriptExecution,
            _ => false
        };
    }
}
