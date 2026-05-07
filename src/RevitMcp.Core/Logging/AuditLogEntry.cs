namespace RevitMcp.Core.Logging;

public sealed record AuditLogEntry(
    DateTimeOffset Timestamp,
    string EventType,
    string? Method = null,
    string? ToolName = null,
    string? ClientName = null,
    string? Message = null,
    double? DurationMs = null,
    double? QueueWaitMs = null,
    bool? IsError = null);
