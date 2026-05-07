using System.Text.Json.Nodes;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Protocol;

namespace RevitMcp.Core.Tools;

public sealed class ElementParametersGetTool : IRevitMcpTool
{
    public string Name => "revit.elements.get_parameters";

    public string Description => "Get parameters from a Revit element.";

    public JsonObject InputSchema => ToolSchema.From(new
    {
        type = "object",
        required = new[] { "elementId" },
        properties = new
        {
            elementId = new { type = "integer" },
            includeBuiltIn = new { type = "boolean", @default = true },
            includeReadOnly = new { type = "boolean", @default = true }
        }
    });

    public ToolAnnotations Annotations => new("Get element parameters", ReadOnlyHint: true);

    public ToolAccessLevel AccessLevel => ToolAccessLevel.Read;

    public bool RequiresActiveDocument => true;

    public McpToolResult Execute(RevitToolContext context, JsonObject arguments)
    {
        var document = context.Document ?? throw new InvalidOperationException("Active document is not available.");
        var elementId = ToolArguments.GetRequiredInt(arguments, "elementId");
        var includeReadOnly = ToolArguments.GetOptionalBoolean(arguments, "includeReadOnly", true);

        var element = document.GetElement(elementId)
            ?? throw new InvalidOperationException($"Element '{elementId}' was not found.");

        var parameters = element.GetParameters()
            .Where(parameter => includeReadOnly || !parameter.IsReadOnly)
            .Select(parameter => new
            {
                name = parameter.Name,
                storageType = parameter.StorageType.ToString(),
                isReadOnly = parameter.IsReadOnly,
                value = ParameterValueSerializer.Serialize(parameter.Value)
            })
            .ToArray();

        return McpToolResult.Success(
            $"Retrieved {parameters.Length} parameter(s) from element {elementId}.",
            new
            {
                elementId,
                parameters
            });
    }
}
