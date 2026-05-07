using System.Text.Json.Nodes;

namespace RevitMcp.Core.Server;

public sealed record RouterResponse(int StatusCode, JsonObject? Payload)
{
    public static RouterResponse Json(int statusCode, JsonObject payload) => new(statusCode, payload);

    public static RouterResponse HttpOnly(int statusCode) => new(statusCode, null);
}
