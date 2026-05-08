using ModelContextProtocol.Protocol;
using RevitMcp.Core.Protocol;
using RevitMcp.Core.Runtime;
using RevitMcp.Server.Hosting;

namespace RevitMcp.Server.Backends;

public sealed class FixtureToolExecutionBackend : IToolExecutionBackend
{
    private readonly RevitMcpRuntime _runtime;
    private readonly CallToolResultMapper _mapper;

    public FixtureToolExecutionBackend(RevitMcpRuntime runtime, CallToolResultMapper mapper)
    {
        _runtime = runtime;
        _mapper = mapper;
    }

    public async Task<CallToolResult> ExecuteAsync(string toolName, object arguments, CancellationToken cancellationToken)
    {
        var jsonArguments = JsonSerializerExtensions.ToJsonObject(arguments);
        var coreResult = await _runtime.ExecutionService.EnqueueAndWaitAsync(toolName, jsonArguments, cancellationToken);
        return _mapper.Map(coreResult);
    }
}
