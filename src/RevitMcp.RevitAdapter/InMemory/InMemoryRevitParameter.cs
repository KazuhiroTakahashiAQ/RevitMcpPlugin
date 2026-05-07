using RevitMcp.Core.Revit;

namespace RevitMcp.RevitAdapter.InMemory;

public sealed class InMemoryRevitParameter : IRevitParameter
{
    public InMemoryRevitParameter(string name, ParameterStorageType storageType, bool isReadOnly, object? value)
    {
        Name = name;
        StorageType = storageType;
        IsReadOnly = isReadOnly;
        Value = value;
    }

    public string Name { get; }

    public ParameterStorageType StorageType { get; }

    public bool IsReadOnly { get; }

    public object? Value { get; private set; }

    public bool TrySetValue(object? value, out string? errorMessage)
    {
        if (IsReadOnly)
        {
            errorMessage = $"Parameter '{Name}' is read-only.";
            return false;
        }

        switch (StorageType)
        {
            case ParameterStorageType.String:
                Value = value?.ToString() ?? string.Empty;
                errorMessage = null;
                return true;
            case ParameterStorageType.Integer when value is int intValue:
                Value = intValue;
                errorMessage = null;
                return true;
            case ParameterStorageType.Double when value is double doubleValue:
                Value = doubleValue;
                errorMessage = null;
                return true;
            case ParameterStorageType.Double when value is int intAsDouble:
                Value = (double)intAsDouble;
                errorMessage = null;
                return true;
            case ParameterStorageType.Boolean when value is bool boolValue:
                Value = boolValue;
                errorMessage = null;
                return true;
            default:
                errorMessage = $"Value '{value}' is not valid for parameter '{Name}'.";
                return false;
        }
    }
}
