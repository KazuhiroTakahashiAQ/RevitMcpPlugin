using System.Text.Json.Nodes;
using RevitMcp.Core.Protocol;

namespace RevitMcp.Core.Tools;

internal static class ToolSchema
{
    public static JsonObject From(object value)
    {
        return JsonSerializerExtensions.ToJsonObject(value);
    }
}
