using System.Text.Json.Nodes;
using RevitMcp.Core.Config;
using RevitMcp.Core.Logging;
using RevitMcp.Core.Runtime;
using RevitMcp.Core.Server;
using RevitMcp.RevitAdapter.InMemory;

namespace RevitMcp.Tests;

public sealed class RevitMcpRuntimeTests
{
    [Fact]
    public async Task Initialize_ReturnsServerInfo()
    {
        var runtime = CreateRuntime(out _);

        var response = await runtime.Router.RouteAsync("""
            {
              "jsonrpc": "2.0",
              "id": 1,
              "method": "initialize",
              "params": {
                "protocolVersion": "2025-06-18"
              }
            }
            """, new HttpRequestContext());

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("revit-mcp", response.Payload?["result"]?["serverInfo"]?["name"]?.GetValue<string>());
    }

    [Fact]
    public async Task ToolsList_ExcludesWriteTools_WhenPolicyDisablesThem()
    {
        var runtime = CreateRuntime(
            out _,
            new RevitMcpSettings
            {
                Tools = new ToolSettings
                {
                    EnableWriteTools = false
                }
            });

        var response = await runtime.Router.RouteAsync("""
            {
              "jsonrpc": "2.0",
              "id": 2,
              "method": "tools/list"
            }
            """, new HttpRequestContext());

        var tools = response.Payload?["result"]?["tools"]?.AsArray() ?? [];
        Assert.DoesNotContain(tools, tool => tool?["name"]?.GetValue<string>() == "revit.wall.create_line");
        Assert.Contains(tools, tool => tool?["name"]?.GetValue<string>() == "revit.document.get_info");
    }

    [Fact]
    public async Task ToolCall_ReturnsStructuredContent_ForReadTool()
    {
        var runtime = CreateRuntime(out _);

        var response = await runtime.Router.RouteAsync("""
            {
              "jsonrpc": "2.0",
              "id": 3,
              "method": "tools/call",
              "params": {
                "name": "revit.document.get_info",
                "arguments": {
                  "includePath": true
                }
              }
            }
            """, new HttpRequestContext(ClientName: "test-client"));

        Assert.Equal(200, response.StatusCode);
        Assert.False(response.Payload?["result"]?["isError"]?.GetValue<bool>());
        Assert.Equal("SampleProject.rvt", response.Payload?["result"]?["structuredContent"]?["title"]?.GetValue<string>());
    }

    [Fact]
    public async Task ToolCall_RejectsDisabledWriteTool()
    {
        var runtime = CreateRuntime(
            out _,
            new RevitMcpSettings
            {
                Tools = new ToolSettings
                {
                    EnableWriteTools = false
                }
            });

        var response = await runtime.Router.RouteAsync("""
            {
              "jsonrpc": "2.0",
              "id": 4,
              "method": "tools/call",
              "params": {
                "name": "revit.wall.create_line",
                "arguments": {
                  "start": { "x": 0, "y": 0, "z": 0 },
                  "end": { "x": 1000, "y": 0, "z": 0 },
                  "levelName": "Level 1"
                }
              }
            }
            """, new HttpRequestContext());

        Assert.Equal(-32002, response.Payload?["error"]?["code"]?.GetValue<int>());
    }

    [Fact]
    public async Task ToolCall_TimesOut_WhenNoDispatcherProcessesQueue()
    {
        var settings = new RevitMcpSettings
        {
            Server = new ServerSettings
            {
                RequestTimeoutSeconds = 5,
                MaxQueueLength = 100,
                Host = "127.0.0.1",
                Port = 4863,
                Endpoint = "/mcp"
            }
        };

        var runtime = RevitMcpRuntime.CreateDefault(new ImmediateExternalEventDispatcher(), settings, NullAuditLogger.Instance);

        var response = await runtime.Router.RouteAsync("""
            {
              "jsonrpc": "2.0",
              "id": 5,
              "method": "tools/call",
              "params": {
                "name": "revit.document.get_info",
                "arguments": {}
              }
            }
            """, new HttpRequestContext());

        Assert.Equal(-32001, response.Payload?["error"]?["code"]?.GetValue<int>());
    }

    [Fact]
    public async Task SetParameter_UpdatesInMemoryElement()
    {
        var runtime = CreateRuntime(out var document);

        var response = await runtime.Router.RouteAsync("""
            {
              "jsonrpc": "2.0",
              "id": 6,
              "method": "tools/call",
              "params": {
                "name": "revit.elements.set_parameter",
                "arguments": {
                  "elementId": 1001,
                  "parameterName": "Comments",
                  "value": "Updated from MCP"
                }
              }
            }
            """, new HttpRequestContext());

        Assert.False(response.Payload?["result"]?["isError"]?.GetValue<bool>());
        Assert.Equal("Updated from MCP", document.GetElement(1001)?.GetParameter("Comments")?.Value);
    }

    private static RevitMcpRuntime CreateRuntime(out InMemoryRevitDocumentContext document, RevitMcpSettings? settings = null)
    {
        settings ??= new RevitMcpSettings();
        var dispatcher = new ImmediateExternalEventDispatcher();
        var logger = NullAuditLogger.Instance;
        var runtime = RevitMcpRuntime.CreateDefault(dispatcher, settings, logger);

        document = CreateDocument();
        var application = new InMemoryRevitApplicationContext(document);
        dispatcher.Bind(ct => runtime.ExecutionService.DrainAsync(application, ct));

        return runtime;
    }

    private static InMemoryRevitDocumentContext CreateDocument()
    {
        var wall = new InMemoryRevitElement(
            id: 1001,
            uniqueId: "wall-1001",
            category: "OST_Walls",
            name: "Basic Wall: Generic - 200mm",
            parameters: new[]
            {
                new InMemoryRevitParameter("Comments", RevitMcp.Core.Revit.ParameterStorageType.String, false, "Initial"),
                new InMemoryRevitParameter("Mark", RevitMcp.Core.Revit.ParameterStorageType.String, false, "W-01")
            });

        var door = new InMemoryRevitElement(
            id: 2001,
            uniqueId: "door-2001",
            category: "OST_Doors",
            name: "Single Flush",
            parameters: new[]
            {
                new InMemoryRevitParameter("Comments", RevitMcp.Core.Revit.ParameterStorageType.String, false, "Door")
            });

        var document = new InMemoryRevitDocumentContext(
            title: "SampleProject.rvt",
            path: "/tmp/SampleProject.rvt",
            isFamilyDocument: false,
            isModified: true,
            isWorkshared: true,
            activeView: new InMemoryRevitView(101, "Level 1", "FloorPlan"),
            elements: new[] { wall, door });

        document.Select(1001, 2001);
        return document;
    }
}
