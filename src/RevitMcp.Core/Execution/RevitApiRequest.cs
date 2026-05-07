using System.Text.Json.Nodes;
using RevitMcp.Core.Protocol;
using RevitMcp.Core.Tools;

namespace RevitMcp.Core.Execution;

public sealed class RevitApiRequest
{
    public Guid RequestId { get; init; } = Guid.NewGuid();
    public string ToolName { get; init; } = string.Empty;
    public JsonObject Arguments { get; init; } = new();
    public ToolAccessLevel AccessLevel { get; init; }
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public TaskCompletionSource<McpToolResult> Completion { get; init; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}
