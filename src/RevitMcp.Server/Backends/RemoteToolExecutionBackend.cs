using ModelContextProtocol.Protocol;
using RevitMcp.Core.Protocol;
using RevitMcp.Server.Hosting;

namespace RevitMcp.Server.Backends;

public sealed class RemoteToolExecutionBackend : IToolExecutionBackend
{
    private readonly RemoteMcpClient _client;
    private readonly CallToolResultMapper _mapper;

    public RemoteToolExecutionBackend(RemoteMcpClient client, CallToolResultMapper mapper)
    {
        _client = client;
        _mapper = mapper;
    }

    public async Task<CallToolResult> ExecuteAsync(string toolName, object arguments, CancellationToken cancellationToken)
    {
        var jsonArguments = JsonSerializerExtensions.ToJsonObject(arguments);
        var result = await _client.CallToolAsync(toolName, jsonArguments, cancellationToken);
        return _mapper.Map(result);
    }
}
