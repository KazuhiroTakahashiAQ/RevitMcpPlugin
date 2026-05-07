using RevitMcp.Core.Revit;

namespace RevitMcp.Core.Tools;

internal static class ElementSummarySerializer
{
    public static object Serialize(IRevitElement element)
    {
        return new
        {
            id = element.Id,
            uniqueId = element.UniqueId,
            category = element.Category,
            name = element.Name
        };
    }
}
