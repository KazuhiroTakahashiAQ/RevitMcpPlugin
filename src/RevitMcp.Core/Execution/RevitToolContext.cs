using RevitMcp.Core.Config;
using RevitMcp.Core.Logging;
using RevitMcp.Core.Revit;

namespace RevitMcp.Core.Execution;

public sealed class RevitToolContext
{
    public RevitToolContext(
        IRevitApplicationContext application,
        RevitMcpSettings settings,
        IAuditLogger logger,
        CancellationToken cancellationToken)
    {
        Application = application;
        Settings = settings;
        Logger = logger;
        CancellationToken = cancellationToken;
    }

    public IRevitApplicationContext Application { get; }

    public IRevitDocumentContext? Document => Application.ActiveDocument;

    public RevitMcpSettings Settings { get; }

    public IAuditLogger Logger { get; }

    public CancellationToken CancellationToken { get; }
}
