using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitMcp.Addin.Runtime;

namespace RevitMcp.Addin.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class StopServerCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
    {
        try
        {
            RevitAddinHost.StopAsync().GetAwaiter().GetResult();
            TaskDialog.Show("Revit MCP", "Revit MCP server stopped.");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            TaskDialog.Show("Revit MCP", $"Failed to stop Revit MCP.\n\n{ex.Message}");
            return Result.Failed;
        }
    }
}
