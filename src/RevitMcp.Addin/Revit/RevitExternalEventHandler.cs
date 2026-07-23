using Autodesk.Revit.UI;
using RevitMcp.Core.Execution;

namespace RevitMcp.Addin.Revit;

public sealed class RevitExternalEventHandler : IExternalEventHandler
{
    private RevitExecutionService? _executionService;

    public void Bind(RevitExecutionService executionService)
    {
        _executionService = executionService;
    }

    public void Execute(UIApplication app)
    {
        if (_executionService is null)
        {
            return;
        }

        var applicationContext = new RevitApplicationContext(app);
        _executionService.Drain(applicationContext, CancellationToken.None);
    }

    public string GetName() => "Revit MCP External Event Handler";
}
