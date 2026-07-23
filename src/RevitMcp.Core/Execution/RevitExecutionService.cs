using System.Diagnostics;
using System.Text.Json.Nodes;
using RevitMcp.Core.Config;
using RevitMcp.Core.Logging;
using RevitMcp.Core.Protocol;
using RevitMcp.Core.Revit;
using RevitMcp.Core.Tools;

namespace RevitMcp.Core.Execution;

public sealed class RevitExecutionService
{
    private readonly RevitApiRequestQueue _queue;
    private readonly ToolRegistry _toolRegistry;
    private readonly RevitMcpSettings _settings;
    private readonly IExternalEventDispatcher _dispatcher;
    private readonly IAuditLogger _logger;

    public RevitExecutionService(
        RevitApiRequestQueue queue,
        ToolRegistry toolRegistry,
        RevitMcpSettings settings,
        IExternalEventDispatcher dispatcher,
        IAuditLogger logger)
    {
        _queue = queue;
        _toolRegistry = toolRegistry;
        _settings = settings;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public int QueueLength => _queue.Count;

    public async Task<McpToolResult> EnqueueAndWaitAsync(string toolName, JsonObject arguments, CancellationToken cancellationToken)
    {
        var tool = _toolRegistry.Get(toolName);

        var request = new RevitApiRequest
        {
            ToolName = toolName,
            Arguments = arguments,
            AccessLevel = tool.AccessLevel,
            Timeout = TimeSpan.FromSeconds(_settings.Server.RequestTimeoutSeconds)
        };

        if (!_queue.TryEnqueue(request))
        {
            throw new InvalidOperationException("Request queue is full.");
        }

        _dispatcher.RequestRaise();

        using var timeoutCancellation = new CancellationTokenSource(request.Timeout);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token);

        try
        {
            return await request.Completion.Task.WaitAsync(linkedCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
        {
            throw new TimeoutException("Tool execution timeout.");
        }
    }

    public void Drain(IRevitApplicationContext applicationContext, CancellationToken cancellationToken = default)
    {
        _dispatcher.OnExecuteStarted();

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var processed = 0;

            while (_queue.TryDequeue(out var request))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (request is null)
                {
                    break;
                }

                if (request.CreatedAt + request.Timeout < DateTimeOffset.UtcNow)
                {
                    request.Completion.TrySetResult(McpToolResult.Error("Tool execution timeout."));
                    continue;
                }

                var queueWait = DateTimeOffset.UtcNow - request.CreatedAt;
                var resultStopwatch = Stopwatch.StartNew();
                var result = ExecuteRequest(applicationContext, request, cancellationToken);
                resultStopwatch.Stop();

                _logger.Log(new AuditLogEntry(
                    Timestamp: DateTimeOffset.UtcNow,
                    EventType: "tools/call",
                    Method: "tools/call",
                    ToolName: request.ToolName,
                    DurationMs: resultStopwatch.Elapsed.TotalMilliseconds,
                    QueueWaitMs: queueWait.TotalMilliseconds,
                    IsError: result.IsError));

                request.Completion.TrySetResult(result);
                processed++;

                if (processed >= _settings.Server.MaxBatchSize)
                {
                    break;
                }

                if (stopwatch.ElapsedMilliseconds >= _settings.Server.MaxExecutionSliceMilliseconds)
                {
                    break;
                }
            }
        }
        finally
        {
            _dispatcher.OnExecuteCompleted(!_queue.IsEmpty);
        }
    }

    private McpToolResult ExecuteRequest(IRevitApplicationContext applicationContext, RevitApiRequest request, CancellationToken cancellationToken)
    {
        var tool = _toolRegistry.Get(request.ToolName);
        var context = new RevitToolContext(applicationContext, _settings, _logger, cancellationToken);

        if (tool.RequiresActiveDocument && context.Document is null)
        {
            return McpToolResult.Error("Active document is not available.");
        }

        try
        {
            if (tool.AccessLevel == ToolAccessLevel.Read)
            {
                return tool.Execute(context, request.Arguments);
            }

            using var transaction = applicationContext.BeginTransaction($"MCP: {tool.Name}");
            try
            {
                var result = tool.Execute(context, request.Arguments);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.RollBack();
                throw;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return McpToolResult.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return McpToolResult.Error(ex.Message);
        }
    }
}
