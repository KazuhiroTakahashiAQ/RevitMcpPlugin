using System.Text.Json.Nodes;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Protocol;

namespace RevitMcp.Core.Tools;

public interface IRevitMcpTool
{
    string Name { get; }

    string Description { get; }

    JsonObject InputSchema { get; }

    ToolAnnotations Annotations { get; }

    ToolAccessLevel AccessLevel { get; }

    bool RequiresActiveDocument { get; }

    McpToolResult Execute(RevitToolContext context, JsonObject arguments);
}
