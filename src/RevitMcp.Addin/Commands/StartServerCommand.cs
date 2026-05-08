using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitMcp.Addin.Runtime;

namespace RevitMcp.Addin.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class StartServerCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
    {
        try
        {
            var endpoint = RevitAddinHost.Start(commandData.Application);
            TaskDialog.Show("Revit MCP", $"Revit MCP server started.\n\nEndpoint: {endpoint}");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            TaskDialog.Show("Revit MCP", $"Failed to start Revit MCP.\n\n{ex.Message}");
            return Result.Failed;
        }
    }
}
