using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace RevitMcp.Core.Server;

public sealed class McpHttpServer : IAsyncDisposable
{
    private readonly HttpListener _listener = new();
    private readonly McpRouter _router;
    private readonly string _expectedPath;
    private CancellationTokenSource? _shutdown;
    private Task? _loopTask;

    public McpHttpServer(McpRouter router, string host, int port, string endpoint)
    {
        _router = router;
        _expectedPath = endpoint.TrimEnd('/');
        var prefix = $"http://{host}:{port}{_expectedPath}/";
        _listener.Prefixes.Add(prefix);
    }

    public bool IsRunning => _listener.IsListening;

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        _shutdown = new CancellationTokenSource();
        _listener.Start();
        _loopTask = Task.Run(() => AcceptLoopAsync(_shutdown.Token));
    }

    public async Task StopAsync()
    {
        if (!IsRunning)
        {
            return;
        }

        _shutdown?.Cancel();
        _listener.Stop();

        if (_loopTask is not null)
        {
            await _loopTask.ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _listener.Close();
        _shutdown?.Dispose();
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            _ = Task.Run(() => HandleRequestAsync(context, cancellationToken), cancellationToken);
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            }

            if (!string.Equals(context.Request.Url?.AbsolutePath.TrimEnd('/'), _expectedPath, StringComparison.Ordinal))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            }

            if (!context.Request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                context.Response.Close();
                return;
            }

            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            var body = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var requestContext = new HttpRequestContext(
                Origin: context.Request.Headers["Origin"],
                Authorization: context.Request.Headers["Authorization"]);

            var response = await _router.RouteAsync(body, requestContext, cancellationToken).ConfigureAwait(false);
            context.Response.StatusCode = response.StatusCode;

            if (response.Payload is not null)
            {
                context.Response.ContentType = "application/json";
                var payload = response.Payload.ToJsonString();
                var bytes = Encoding.UTF8.GetBytes(payload);
                await context.Response.OutputStream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
            }

            context.Response.Close();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[RevitMcp] Unhandled exception while handling request: {ex}");

            try
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }
            catch (Exception closeEx)
            {
                Console.Error.WriteLine($"[RevitMcp] Failed to close response after error: {closeEx}");
            }
        }
    }
}
