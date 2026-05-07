using System.Text.Json.Nodes;

namespace RevitMcp.Core.Protocol;

public static class JsonRpcResponseFactory
{
    public static JsonObject Result(JsonNode? id, object? result)
    {
        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["result"] = result is null ? null : JsonSerializerExtensions.ToJsonNode(result)
        };
    }

    public static JsonObject Error(JsonNode? id, int code, string message, object? data = null)
    {
        var error = new JsonObject
        {
            ["code"] = code,
            ["message"] = message
        };

        if (data is not null)
        {
            error["data"] = JsonSerializerExtensions.ToJsonNode(data);
        }

        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["error"] = error
        };
    }
}
