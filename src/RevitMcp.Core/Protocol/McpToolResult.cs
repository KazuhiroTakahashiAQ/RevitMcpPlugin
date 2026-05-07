using System.Text.Json.Nodes;

namespace RevitMcp.Core.Protocol;

public sealed record McpToolResult(IReadOnlyList<McpToolContent> Content, JsonNode? StructuredContent, bool IsError)
{
    public static McpToolResult Success(string text, object? structuredContent = null)
    {
        return new McpToolResult(
            new[] { McpToolContent.TextContent(text) },
            structuredContent is null ? null : JsonSerializerExtensions.ToJsonNode(structuredContent),
            false);
    }

    public static McpToolResult Error(string text, object? structuredContent = null)
    {
        return new McpToolResult(
            new[] { McpToolContent.TextContent(text) },
            structuredContent is null ? null : JsonSerializerExtensions.ToJsonNode(structuredContent),
            true);
    }
}
