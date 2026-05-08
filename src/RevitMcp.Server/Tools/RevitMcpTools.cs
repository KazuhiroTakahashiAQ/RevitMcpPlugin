using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using RevitMcp.Server.Backends;
using System.ComponentModel;

namespace RevitMcp.Server.Tools;

[McpServerToolType]
public sealed class RevitMcpTools
{
    [McpServerTool(
        Name = "revit.document.get_info",
        Title = "Get document info",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Get information about the active Revit document.")]
    public Task<CallToolResult> GetDocumentInfo(
        [Description("Include worksharing details in the response.")] bool includeWorksharing = true,
        [Description("Include the document path in the response.")] bool includePath = false,
        IToolExecutionBackend executor = null!,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync(
            "revit.document.get_info",
            new
            {
                includeWorksharing,
                includePath
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.selection.get",
        Title = "Get selection",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Get selected elements from the active document.")]
    public Task<CallToolResult> GetSelection(
        [Description("Include element parameters in the response.")] bool includeParameters = false,
        [Description("Maximum number of parameters to include per element when includeParameters is true.")] int parameterLimit = 50,
        IToolExecutionBackend executor = null!,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync(
            "revit.selection.get",
            new
            {
                includeParameters,
                parameterLimit
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.elements.find",
        Title = "Find elements",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Find elements by category, name, and parameter filters.")]
    public Task<CallToolResult> FindElements(
        [Description("Optional BuiltInCategory name. Example: OST_Walls.")] string? category = null,
        [Description("Optional case-insensitive substring match against element names.")] string? nameContains = null,
        [Description("Optional parameter name for exact string matching.")] string? parameterName = null,
        [Description("Optional parameter value for exact string matching.")] string? parameterValue = null,
        [Description("Maximum number of elements to return.")] int limit = 100,
        IToolExecutionBackend executor = null!,
        CancellationToken cancellationToken = default)
    {
        object? parameterEquals = null;
        if (!string.IsNullOrWhiteSpace(parameterName) && !string.IsNullOrWhiteSpace(parameterValue))
        {
            parameterEquals = new
            {
                name = parameterName,
                value = parameterValue
            };
        }

        return executor.ExecuteAsync(
            "revit.elements.find",
            new
            {
                category,
                nameContains,
                parameterEquals,
                limit
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.elements.get_parameters",
        Title = "Get element parameters",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Get parameters from a Revit element.")]
    public Task<CallToolResult> GetParameters(
        [Description("Element id.")] int elementId,
        [Description("Include built-in parameters when available.")] bool includeBuiltIn = true,
        [Description("Include read-only parameters in the response.")] bool includeReadOnly = true,
        IToolExecutionBackend executor = null!,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync(
            "revit.elements.get_parameters",
            new
            {
                elementId,
                includeBuiltIn,
                includeReadOnly
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.elements.set_parameter",
        Title = "Set element parameter",
        ReadOnly = false,
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Set a parameter value on a Revit element.")]
    public Task<CallToolResult> SetParameter(
        [Description("Element id.")] int elementId,
        [Description("Parameter name.")] string parameterName,
        [Description("Parameter value as string, number, integer, or boolean.")] object value,
        IToolExecutionBackend executor = null!,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync(
            "revit.elements.set_parameter",
            new
            {
                elementId,
                parameterName,
                value
            },
            cancellationToken);
    }

    [McpServerTool(
        Name = "revit.wall.create_line",
        Title = "Create line wall",
        ReadOnly = false,
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Create a wall on a line in the active document.")]
    public Task<CallToolResult> CreateWallLine(
        [Description("Start point X in millimeters.")] double startX,
        [Description("Start point Y in millimeters.")] double startY,
        [Description("Start point Z in millimeters.")] double startZ,
        [Description("End point X in millimeters.")] double endX,
        [Description("End point Y in millimeters.")] double endY,
        [Description("End point Z in millimeters.")] double endZ,
        [Description("Level name.")] string levelName,
        [Description("Optional wall type name.")] string? wallTypeName = null,
        [Description("Wall height in millimeters.")] double heightMm = 3000,
        IToolExecutionBackend executor = null!,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync(
            "revit.wall.create_line",
            new
            {
                start = new { x = startX, y = startY, z = startZ },
                end = new { x = endX, y = endY, z = endZ },
                levelName,
                wallTypeName,
                heightMm
            },
            cancellationToken);
    }
}
