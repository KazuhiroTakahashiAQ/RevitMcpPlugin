using Autodesk.Revit.UI;

namespace RevitMcp.Addin.UI;

public static class RibbonBuilder
{
    private const string TabName = "AsiaQuest";
    private const string PanelName = "Revit MCP";

    public static void Create(UIControlledApplication application)
    {
        try
        {
            application.CreateRibbonTab(TabName);
        }
        catch
        {
        }

        var panel = application.GetRibbonPanels(TabName)
            .FirstOrDefault(existing => string.Equals(existing.Name, PanelName, StringComparison.Ordinal))
            ?? application.CreateRibbonPanel(TabName, PanelName);

        AddButton<Commands.StartServerCommand>(panel, "StartRevitMcp", "Start MCP", "Start the Revit MCP localhost server.");
        AddButton<Commands.StopServerCommand>(panel, "StopRevitMcp", "Stop MCP", "Stop the Revit MCP localhost server.");
    }

    private static void AddButton<TCommand>(RibbonPanel panel, string name, string text, string tooltip)
    {
        if (panel.GetItems().Any(item => string.Equals(item.Name, name, StringComparison.Ordinal)))
        {
            return;
        }

        var commandType = typeof(TCommand);
        var data = new PushButtonData(
            name,
            text,
            commandType.Assembly.Location,
            commandType.FullName!)
        {
            ToolTip = tooltip
        };

        panel.AddItem(data);
    }
}
