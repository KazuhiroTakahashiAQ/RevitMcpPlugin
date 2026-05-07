using System.Text.Json.Nodes;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Protocol;

namespace RevitMcp.Core.Tools;

public sealed class ElementParameterSetTool : IRevitMcpTool
{
    public string Name => "revit.elements.set_parameter";

    public string Description => "Set a parameter value on a Revit element.";

    public JsonObject InputSchema => ToolSchema.From(new
    {
        type = "object",
        required = new[] { "elementId", "parameterName", "value" },
        properties = new
        {
            elementId = new { type = "integer" },
            parameterName = new { type = "string" },
            value = new
            {
                oneOf = new object[]
                {
                    new { type = "string" },
                    new { type = "number" },
                    new { type = "integer" },
                    new { type = "boolean" }
                }
            }
        }
    });

    public ToolAnnotations Annotations => new("Set parameter", ReadOnlyHint: false);

    public ToolAccessLevel AccessLevel => ToolAccessLevel.Modify;

    public bool RequiresActiveDocument => true;

    public McpToolResult Execute(RevitToolContext context, JsonObject arguments)
    {
        var document = context.Document ?? throw new InvalidOperationException("Active document is not available.");
        var elementId = ToolArguments.GetRequiredInt(arguments, "elementId");
        var parameterName = ToolArguments.GetRequiredString(arguments, "parameterName");
        var valueNode = ToolArguments.GetRequiredNode(arguments, "value");

        var element = document.GetElement(elementId)
            ?? throw new InvalidOperationException($"Element '{elementId}' was not found.");

        var parameter = element.GetParameter(parameterName)
            ?? throw new InvalidOperationException($"Parameter '{parameterName}' was not found.");

        if (!parameter.TrySetValue(ToolArguments.ToRawValue(valueNode), out var errorMessage))
        {
            throw new InvalidOperationException(errorMessage ?? $"Failed to set parameter '{parameterName}'.");
        }

        return McpToolResult.Success(
            $"Updated parameter '{parameterName}' on element {elementId}.",
            new
            {
                elementId,
                parameterName,
                value = ParameterValueSerializer.Serialize(parameter.Value)
            });
    }
}
