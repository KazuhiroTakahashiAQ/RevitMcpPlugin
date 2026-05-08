using Microsoft.Extensions.DependencyInjection;
using RevitMcp.Core.Config;
using RevitMcp.Core.Execution;
using RevitMcp.Core.Logging;
using RevitMcp.Core.Runtime;
using RevitMcp.RevitAdapter.InMemory;
using RevitMcp.Server.Hosting;

namespace RevitMcp.Server.Backends;

public static class FixtureBackendFactory
{
    public static IToolExecutionBackend Create(IServiceProvider services, ServerLaunchOptions launchOptions)
    {
        var logger = services.GetRequiredService<IAuditLogger>();
        var mapper = services.GetRequiredService<CallToolResultMapper>();
        var fixturePath = string.IsNullOrWhiteSpace(launchOptions.FixturePath)
            ? ServerLaunchOptions.GetBundledFixturePath()
            : launchOptions.FixturePath;

        var document = InMemorySampleProjectFactory.LoadFromFile(fixturePath);
        var application = new InMemoryRevitApplicationContext(document);
        var dispatcher = new ImmediateExternalEventDispatcher();
        var runtime = RevitMcpRuntime.CreateDefault(dispatcher, new RevitMcpSettings(), logger);
        dispatcher.Bind(ct => runtime.ExecutionService.DrainAsync(application, ct));

        return new FixtureToolExecutionBackend(runtime, mapper);
    }
}
