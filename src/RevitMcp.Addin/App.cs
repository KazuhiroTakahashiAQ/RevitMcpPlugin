using Autodesk.Revit.UI;
using RevitMcp.Addin.Runtime;
using RevitMcp.Addin.UI;

namespace RevitMcp.Addin;

public sealed class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        RibbonBuilder.Create(application);
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        RevitAddinHost.StopAsync().GetAwaiter().GetResult();
        return Result.Succeeded;
    }
}
