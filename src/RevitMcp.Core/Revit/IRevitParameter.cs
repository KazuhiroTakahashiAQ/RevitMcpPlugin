namespace RevitMcp.Core.Revit;

public interface IRevitParameter
{
    string Name { get; }
    ParameterStorageType StorageType { get; }
    bool IsReadOnly { get; }
    object? Value { get; }

    bool TrySetValue(object? value, out string? errorMessage);
}
