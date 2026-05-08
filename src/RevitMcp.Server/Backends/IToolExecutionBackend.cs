using ModelContextProtocol.Protocol;

namespace RevitMcp.Server.Backends;

public interface IToolExecutionBackend
{
    Task<CallToolResult> ExecuteAsync(string toolName, object arguments, CancellationToken cancellationToken);
}
