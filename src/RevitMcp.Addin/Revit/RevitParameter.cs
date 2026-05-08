using Autodesk.Revit.DB;
using RevitMcp.Core.Revit;
using CoreParameterStorageType = RevitMcp.Core.Revit.ParameterStorageType;

namespace RevitMcp.Addin.Revit;

public sealed class RevitParameter : IRevitParameter
{
    private readonly Parameter _parameter;

    public RevitParameter(Parameter parameter)
    {
        _parameter = parameter;
    }

    public string Name => _parameter.Definition?.Name ?? string.Empty;

    public CoreParameterStorageType StorageType => _parameter.StorageType switch
    {
        Autodesk.Revit.DB.StorageType.String => CoreParameterStorageType.String,
        Autodesk.Revit.DB.StorageType.Integer => CoreParameterStorageType.Integer,
        Autodesk.Revit.DB.StorageType.Double => CoreParameterStorageType.Double,
        _ => CoreParameterStorageType.String
    };

    public bool IsReadOnly => _parameter.IsReadOnly;

    public object? Value => ParameterValueReader.Read(_parameter);

    public bool TrySetValue(object? value, out string? errorMessage)
    {
        if (_parameter.IsReadOnly)
        {
            errorMessage = $"Parameter '{Name}' is read-only.";
            return false;
        }

        try
        {
            switch (_parameter.StorageType)
            {
                case Autodesk.Revit.DB.StorageType.String:
                    _parameter.Set(value?.ToString() ?? string.Empty);
                    break;
                case Autodesk.Revit.DB.StorageType.Integer when value is int integerValue:
                    _parameter.Set(integerValue);
                    break;
                case Autodesk.Revit.DB.StorageType.Integer when value is bool boolValue:
                    _parameter.Set(boolValue ? 1 : 0);
                    break;
                case Autodesk.Revit.DB.StorageType.Double when value is double doubleValue:
                    _parameter.Set(doubleValue);
                    break;
                case Autodesk.Revit.DB.StorageType.Double when value is int integerAsDouble:
                    _parameter.Set(integerAsDouble);
                    break;
                default:
                    errorMessage = $"Value '{value}' is not valid for parameter '{Name}'.";
                    return false;
            }

            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}
