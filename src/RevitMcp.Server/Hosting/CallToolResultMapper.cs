using ModelContextProtocol.Protocol;
using RevitMcp.Core.Protocol;
using System.Text.Json;

namespace RevitMcp.Server.Hosting;

public sealed class CallToolResultMapper
{
    public CallToolResult Map(McpToolResult result)
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
