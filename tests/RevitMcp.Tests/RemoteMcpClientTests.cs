using System.Net.Sockets;
using RevitMcp.Core.Config;
using RevitMcp.Core.Logging;
using RevitMcp.Core.Runtime;
using RevitMcp.Core.Server;
using RevitMcp.RevitAdapter.InMemory;
using RevitMcp.Server.Backends;

namespace RevitMcp.Tests;

public sealed class RemoteMcpClientTests
{
    [Fact]
    public async Task CallToolAsync_ReturnsResult_FromHttpEndpoint()
    {
        var port = GetAvailablePort();
        var settings = new RevitMcpSettings
        {
            Server = new ServerSettings
            {
                Host = "127.0.0.1",
                Port = port,
                Endpoint = "/mcp",
                RequestTimeoutSeconds = 30,
                MaxQueueLength = 100
            }
        };

        var runtime = CreateRuntime(settings);
        await using var server = new McpHttpServer(runtime.Router, settings.Server.Host, settings.Server.Port, settings.Server.Endpoint);
        server.Start();

        var client = new RemoteMcpClient(new HttpClient(), new Uri($"http://127.0.0.1:{port}/mcp"), bearerToken: null);
        var result = await client.CallToolAsync("revit.document.get_info", new System.Text.Json.Nodes.JsonObject(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal("SampleProject.rvt", result.StructuredContent?["title"]?.GetValue<string>());
    }

    [Fact]
    public async Task CallToolAsync_Throws_WhenBearerTokenIsRejected()
    {
        var port = GetAvailablePort();
        var settings = new RevitMcpSettings
        {
            Server = new ServerSettings
            {
                Host = "127.0.0.1",
                Port = port,
                Endpoint = "/mcp",
                RequestTimeoutSeconds = 30,
                MaxQueueLength = 100,
                RequireAuthToken = true,
                AuthToken = "expected-token"
            }
        };

        var runtime = CreateRuntime(settings);
        await using var server = new McpHttpServer(runtime.Router, settings.Server.Host, settings.Server.Port, settings.Server.Endpoint);
        server.Start();

        var client = new RemoteMcpClient(new HttpClient(), new Uri($"http://127.0.0.1:{port}/mcp"), bearerToken: "wrong-token");
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.CallToolAsync("revit.document.get_info", new System.Text.Json.Nodes.JsonObject(), CancellationToken.None));

        Assert.Contains("HTTP 401", exception.Message);
    }

    private static RevitMcpRuntime CreateRuntime(RevitMcpSettings settings)
    {
        var dispatcher = new ImmediateExternalEventDispatcher();
        var runtime = RevitMcpRuntime.CreateDefault(dispatcher, settings, NullAuditLogger.Instance);
        var application = new InMemoryRevitApplicationContext(InMemorySampleProjectFactory.CreateDefault());
        dispatcher.Bind(ct => runtime.ExecutionService.DrainAsync(application, ct));
        return runtime;
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }
}
