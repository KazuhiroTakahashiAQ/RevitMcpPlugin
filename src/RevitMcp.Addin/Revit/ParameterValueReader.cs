using Autodesk.Revit.DB;

namespace RevitMcp.Addin.Revit;

internal static class ParameterValueReader
{
    public static object? Read(Parameter parameter)
    {
        return parameter.StorageType switch
        {
            StorageType.String => parameter.AsString(),
            StorageType.Integer => parameter.AsInteger(),
            StorageType.Double => parameter.AsDouble(),
            StorageType.ElementId => (int)parameter.AsElementId().Value,
            _ => parameter.AsValueString() ?? parameter.AsString()
        };
    }

    public static string ReadAsString(Parameter parameter)
    {
        return parameter.AsValueString()
            ?? parameter.AsString()
            ?? parameter.StorageType switch
            {
                StorageType.Integer => parameter.AsInteger().ToString(),
                StorageType.Double => parameter.AsDouble().ToString("G"),
                StorageType.ElementId => parameter.AsElementId().Value.ToString(),
                _ => string.Empty
            };
    }
}
