namespace RevitMcp.Core.Logging;

public sealed class NullAuditLogger : IAuditLogger
{
    public static readonly NullAuditLogger Instance = new();

    private NullAuditLogger()
    {
    }

    public void Log(AuditLogEntry entry)
    {
    }
}
