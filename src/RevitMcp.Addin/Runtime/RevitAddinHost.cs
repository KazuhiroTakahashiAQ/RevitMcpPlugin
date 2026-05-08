using Autodesk.Revit.UI;
using RevitMcp.Addin.Logging;
using RevitMcp.Addin.Revit;
using RevitMcp.Core.Config;
using RevitMcp.Core.Runtime;
using RevitMcp.Core.Server;

namespace RevitMcp.Addin.Runtime;

public static class RevitAddinHost
{
    private static readonly object Gate = new();
    private static RevitMcpRuntime? _runtime;
    private static McpHttpServer? _server;

    public static bool IsRunning
    {
        get
        {
            lock (Gate)
            {
                return _server?.IsRunning == true;
            }
        }
    }

    public static string Start(UIApplication application)
    {
        lock (Gate)
        {
            if (_server?.IsRunning == true)
            {
                return BuildEndpoint(_runtime!.Settings);
            }

            var settings = new RevitMcpSettings();
            var logger = new FileAuditLogger(GetLogDirectory());
            var handler = new RevitExternalEventHandler();
            var externalEvent = ExternalEvent.Create(handler);
            var dispatcher = new RevitExternalEventDispatcher(externalEvent);
            var runtime = RevitMcpRuntime.CreateDefault(dispatcher, settings, logger);
            handler.Bind(runtime.ExecutionService);

            var server = new McpHttpServer(
                runtime.Router,
                settings.Server.Host,
                settings.Server.Port,
                settings.Server.Endpoint);

            server.Start();

            _runtime = runtime;
            _server = server;

            return BuildEndpoint(settings);
        }
    }

    public static async Task StopAsync()
    {
        McpHttpServer? serverToStop;

        lock (Gate)
        {
            serverToStop = _server;
            _server = null;
            _runtime = null;
        }

        if (serverToStop is not null)
        {
            await serverToStop.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static string GetLogDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "AsiaQuest", "RevitMcp", "logs");
    }

    private static string BuildEndpoint(RevitMcpSettings settings)
    {
        return $"http://{settings.Server.Host}:{settings.Server.Port}{settings.Server.Endpoint}";
    }
}
