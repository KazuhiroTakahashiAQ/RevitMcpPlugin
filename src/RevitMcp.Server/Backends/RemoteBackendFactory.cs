using Microsoft.Extensions.DependencyInjection;
using RevitMcp.Server.Hosting;

namespace RevitMcp.Server.Backends;

public static class RemoteBackendFactory
{
    public static IToolExecutionBackend Create(IServiceProvider services, ServerLaunchOptions launchOptions)
    {
        var mapper = services.GetRequiredService<CallToolResultMapper>();
        var client = new RemoteMcpClient(
            new HttpClient(),
            new Uri(launchOptions.RemoteUrl!, UriKind.Absolute),
            launchOptions.BearerToken);

        return new RemoteToolExecutionBackend(client, mapper);
    }
}
