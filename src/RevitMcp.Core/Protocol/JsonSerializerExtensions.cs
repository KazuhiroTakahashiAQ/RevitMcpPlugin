using System.Text.Json;
using System.Text.Json.Nodes;

namespace RevitMcp.Core.Protocol;

public static class JsonSerializerExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web);

    public static JsonNode ToJsonNode(object value)
    {
        return JsonSerializer.SerializeToNode(value, DefaultOptions)
            ?? throw new InvalidOperationException("Failed to serialize JSON node.");
    }

    public static JsonObject ToJsonObject(object value)
    {
        return ToJsonNode(value).AsObject();
    }
}
