using System.Text.Json.Nodes;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Protocol;
using RevitMcp.Core.Revit;

namespace RevitMcp.Core.Tools;

public sealed class ElementsFindTool : IRevitMcpTool
{
    public string Name => "revit.elements.find";

    public string Description => "Find elements by category, name, and parameter filters.";

    public JsonObject InputSchema => ToolSchema.From(new
    {
        type = "object",
        properties = new
        {
            category = new
            {
                type = "string",
                description = "BuiltInCategory name. Example: OST_Walls"
            },
            nameContains = new { type = "string" },
            parameterEquals = new
            {
                type = "object",
                properties = new
                {
                    name = new { type = "string" },
                    value = new { type = "string" }
                }
            },
            limit = new
            {
                type = "integer",
                @default = 100,
                minimum = 1,
                maximum = 1000
            }
        }
    });

    public ToolAnnotations Annotations => new("Find elements", ReadOnlyHint: true);

    public ToolAccessLevel AccessLevel => ToolAccessLevel.Read;

    public bool RequiresActiveDocument => true;

    public McpToolResult Execute(RevitToolContext context, JsonObject arguments)
    {
        var document = context.Document ?? throw new InvalidOperationException("Active document is not available.");
        var limit = Math.Min(
            ToolArguments.GetOptionalInt(arguments, "limit", 100),
            context.Settings.Tools.MaxFindElementsLimit);

        RevitParameterMatch? parameterMatch = null;
        if (arguments["parameterEquals"] is JsonObject parameterFilter)
        {
            parameterMatch = new RevitParameterMatch(
                ToolArguments.GetRequiredString(parameterFilter, "name"),
                ToolArguments.GetRequiredString(parameterFilter, "value"));
        }

        var query = new RevitElementQuery(
            Category: ToolArguments.GetOptionalString(arguments, "category"),
            NameContains: ToolArguments.GetOptionalString(arguments, "nameContains"),
            ParameterEquals: parameterMatch,
            Limit: limit);

        var elements = document.FindElements(query)
            .Select(ElementSummarySerializer.Serialize)
            .ToArray();

        return McpToolResult.Success(
            $"Found {elements.Length} element(s).",
            new
            {
                count = elements.Length,
                elements
            });
    }
}
