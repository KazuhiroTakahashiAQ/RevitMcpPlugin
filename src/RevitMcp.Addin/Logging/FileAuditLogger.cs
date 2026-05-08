using RevitMcp.Core.Logging;
using System.Text;

namespace RevitMcp.Addin.Logging;

public sealed class FileAuditLogger : IAuditLogger
{
    private readonly string _logFilePath;
    private readonly object _gate = new();

    public FileAuditLogger(string rootDirectory)
    {
        Directory.CreateDirectory(rootDirectory);
        _logFilePath = Path.Combine(rootDirectory, $"revit-mcp-{DateTime.Now:yyyyMMdd}.log");
    }

    public void Log(AuditLogEntry entry)
    {
        var line = new StringBuilder()
            .Append(entry.Timestamp.ToString("O"))
            .Append('\t')
            .Append(entry.EventType)
            .Append('\t')
            .Append(entry.Method ?? "-")
            .Append('\t')
            .Append(entry.ToolName ?? "-")
            .Append('\t')
            .Append(entry.ClientName ?? "-")
            .Append('\t')
            .Append(entry.DurationMs?.ToString("0.###") ?? "-")
            .Append('\t')
            .Append(entry.QueueWaitMs?.ToString("0.###") ?? "-")
            .Append('\t')
            .Append(entry.IsError?.ToString() ?? "-")
            .Append('\t')
            .Append(entry.Message ?? "-")
            .ToString();

        lock (_gate)
        {
            File.AppendAllLines(_logFilePath, new[] { line });
        }
    }
}
