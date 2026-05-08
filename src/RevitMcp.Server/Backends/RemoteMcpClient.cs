using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using RevitMcp.Core.Protocol;

namespace RevitMcp.Server.Backends;

public sealed class RemoteMcpClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly Uri _endpoint;
    private readonly string? _bearerToken;

    public RemoteMcpClient(HttpClient httpClient, Uri endpoint, string? bearerToken)
    {
        _httpClient = httpClient;
        _endpoint = endpoint;
        _bearerToken = bearerToken;
    }

    public async Task<McpToolResult> CallToolAsync(string toolName, JsonObject arguments, CancellationToken cancellationToken)
    {
        var request = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = Guid.NewGuid().ToString("N"),
            ["method"] = "tools/call",
            ["params"] = new JsonObject
            {
                ["name"] = toolName,
                ["arguments"] = arguments.DeepClone()
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(request.ToJsonString(), Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(_bearerToken))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
        }

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Remote MCP endpoint returned HTTP {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
        }

        var payload = JsonNode.Parse(body)?.AsObject()
            ?? throw new InvalidOperationException("Remote MCP endpoint returned invalid JSON.");

        if (payload["error"] is JsonObject error)
        {
            var message = error["message"]?.GetValue<string>() ?? "Unknown remote MCP error.";
            var code = error["code"]?.GetValue<int>();
            throw new InvalidOperationException(
                code is null ? message : $"Remote MCP error {code}: {message}");
        }

        if (payload["result"] is not JsonObject result)
        {
            throw new InvalidOperationException("Remote MCP endpoint returned no result payload.");
        }

        return ParseToolResult(result);
    }

    private static McpToolResult ParseToolResult(JsonObject result)
    {
        var content = new List<McpToolContent>();
        if (result["content"] is JsonArray contentArray)
        {
            foreach (var node in contentArray)
            {
                if (node is not JsonObject block)
                {
                    continue;
                }

                var type = block["type"]?.GetValue<string>() ?? "text";
                var text = block["text"]?.GetValue<string>() ?? string.Empty;
                content.Add(new McpToolContent(type, text));
            }
        }

        var structuredContent = result["structuredContent"]?.DeepClone();
        var isError = result["isError"]?.GetValue<bool>() ?? false;

        return new McpToolResult(content, structuredContent, isError);
    }
}
