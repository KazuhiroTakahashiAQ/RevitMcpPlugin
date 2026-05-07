namespace RevitMcp.Core.Logging;

public interface IAuditLogger
{
    void Log(AuditLogEntry entry);
}
