using System.Text.Json.Nodes;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Protocol;

namespace RevitMcp.Core.Tools;

public sealed class DocumentInfoTool : IRevitMcpTool
{
    public string Name => "revit.document.get_info";

    public string Description => "Get information about the active Revit document.";

    public JsonObject InputSchema => ToolSchema.From(new
    {
        type = "object",
        properties = new
        {
            includeWorksharing = new { type = "boolean", @default = true },
            includePath = new { type = "boolean", @default = false }
        }
    });

    public ToolAnnotations Annotations => new("Get document info", ReadOnlyHint: true);

    public ToolAccessLevel AccessLevel => ToolAccessLevel.Read;

    public bool RequiresActiveDocument => true;

    public McpToolResult Execute(RevitToolContext context, JsonObject arguments)
    {
        var document = context.Document ?? throw new InvalidOperationException("Active document is not available.");
        var includePath = ToolArguments.GetOptionalBoolean(arguments, "includePath", false);
        var includeWorksharing = ToolArguments.GetOptionalBoolean(arguments, "includeWorksharing", true);

        return McpToolResult.Success(
            $"Active document: {document.Title}",
            new
            {
                title = document.Title,
                path = includePath ? document.Path : null,
                isFamilyDocument = document.IsFamilyDocument,
                isModified = document.IsModified,
                isWorkshared = includeWorksharing && document.IsWorkshared,
                activeView = new
                {
                    id = document.ActiveView.Id,
                    name = document.ActiveView.Name,
                    type = document.ActiveView.ViewType
                }
            });
    }
}
