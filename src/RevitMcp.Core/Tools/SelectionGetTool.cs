using System.Text.Json.Nodes;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Protocol;

namespace RevitMcp.Core.Tools;

public sealed class SelectionGetTool : IRevitMcpTool
{
    public string Name => "revit.selection.get";

    public string Description => "Get selected elements from the active document.";

    public JsonObject InputSchema => ToolSchema.From(new
    {
        type = "object",
        properties = new
        {
            includeParameters = new { type = "boolean", @default = false },
            parameterLimit = new
            {
                type = "integer",
                @default = 50,
                minimum = 1,
                maximum = 200
            }
        }
    });

    public ToolAnnotations Annotations => new("Get selection", ReadOnlyHint: true);

    public ToolAccessLevel AccessLevel => ToolAccessLevel.Read;

    public bool RequiresActiveDocument => true;

    public McpToolResult Execute(RevitToolContext context, JsonObject arguments)
    {
        var document = context.Document ?? throw new InvalidOperationException("Active document is not available.");
        var includeParameters = ToolArguments.GetOptionalBoolean(arguments, "includeParameters", false);
        var parameterLimit = ToolArguments.GetOptionalInt(arguments, "parameterLimit", 50);

        var elements = document.GetSelectedElements()
            .Select(element => new
            {
                id = element.Id,
                uniqueId = element.UniqueId,
                category = element.Category,
                name = element.Name,
                parameters = includeParameters
                    ? element.GetParameters()
                        .Take(parameterLimit)
                        .Select(parameter => new
                        {
                            name = parameter.Name,
                            storageType = parameter.StorageType.ToString(),
                            isReadOnly = parameter.IsReadOnly,
                            value = ParameterValueSerializer.Serialize(parameter.Value)
                        })
                        .ToArray()
                    : null
            })
            .ToArray();

        return McpToolResult.Success(
            $"Selected {elements.Length} element(s).",
            new
            {
                count = elements.Length,
                elements
            });
    }
}
