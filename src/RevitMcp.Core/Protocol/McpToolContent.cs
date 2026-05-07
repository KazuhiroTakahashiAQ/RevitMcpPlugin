namespace RevitMcp.Core.Protocol;

public sealed record McpToolContent(string Type, string Text)
{
    public static McpToolContent TextContent(string text) => new("text", text);
}
