using System.Text.Json.Nodes;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Protocol;
using RevitMcp.Core.Revit;

namespace RevitMcp.Core.Tools;

public sealed class WallCreateLineTool : IRevitMcpTool
{
    public string Name => "revit.wall.create_line";

    public string Description => "Create a wall on a line in the active document.";

    public JsonObject InputSchema => ToolSchema.From(new
    {
        type = "object",
        required = new[] { "start", "end", "levelName" },
        properties = new
        {
            start = new
            {
                type = "object",
                required = new[] { "x", "y", "z" },
                properties = new
                {
                    x = new { type = "number" },
                    y = new { type = "number" },
                    z = new { type = "number" }
                }
            },
            end = new
            {
                type = "object",
                required = new[] { "x", "y", "z" },
                properties = new
                {
                    x = new { type = "number" },
                    y = new { type = "number" },
                    z = new { type = "number" }
                }
            },
            levelName = new { type = "string" },
            wallTypeName = new { type = "string" },
            heightMm = new { type = "number", @default = 3000 }
        }
    });

    public ToolAnnotations Annotations => new("Create line wall", ReadOnlyHint: false);

    public ToolAccessLevel AccessLevel => ToolAccessLevel.Modify;

    public bool RequiresActiveDocument => true;

    public McpToolResult Execute(RevitToolContext context, JsonObject arguments)
    {
        var document = context.Document ?? throw new InvalidOperationException("Active document is not available.");
        var start = ReadPoint(ToolArguments.GetRequiredObject(arguments, "start"));
        var end = ReadPoint(ToolArguments.GetRequiredObject(arguments, "end"));
        var levelName = ToolArguments.GetRequiredString(arguments, "levelName");
        var wallTypeName = ToolArguments.GetOptionalString(arguments, "wallTypeName");
        var heightMm = arguments["heightMm"]?.GetValue<double>() ?? 3000d;

        var wall = document.CreateWallLine(new RevitLineWallDefinition(start, end, levelName, wallTypeName, heightMm));

        return McpToolResult.Success(
            $"Created wall {wall.Id}.",
            new
            {
                id = wall.Id,
                uniqueId = wall.UniqueId,
                category = wall.Category,
                name = wall.Name
            });
    }

    private static RevitPointMm ReadPoint(JsonObject point)
    {
        return new RevitPointMm(
            ToolArguments.GetRequiredDouble(point, "x"),
            ToolArguments.GetRequiredDouble(point, "y"),
            ToolArguments.GetRequiredDouble(point, "z"));
    }
}
