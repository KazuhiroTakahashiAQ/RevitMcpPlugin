using System.Text.Json.Nodes;

namespace RevitMcp.Core.Tools;

internal static class ToolArguments
{
    public static bool GetOptionalBoolean(JsonObject arguments, string propertyName, bool defaultValue)
    {
        return arguments[propertyName]?.GetValue<bool>() ?? defaultValue;
    }

    public static int GetOptionalInt(JsonObject arguments, string propertyName, int defaultValue)
    {
        return arguments[propertyName]?.GetValue<int>() ?? defaultValue;
    }

    public static string? GetOptionalString(JsonObject arguments, string propertyName)
    {
        return arguments[propertyName]?.GetValue<string>();
    }

    public static int GetRequiredInt(JsonObject arguments, string propertyName)
    {
        return arguments[propertyName]?.GetValue<int>()
            ?? throw new ArgumentException($"Argument '{propertyName}' is required.");
    }

    public static double GetRequiredDouble(JsonObject arguments, string propertyName)
    {
        return arguments[propertyName]?.GetValue<double>()
            ?? throw new ArgumentException($"Argument '{propertyName}' is required.");
    }

    public static string GetRequiredString(JsonObject arguments, string propertyName)
    {
        var value = arguments[propertyName]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Argument '{propertyName}' is required.");
        }

        return value;
    }

    public static JsonObject GetRequiredObject(JsonObject arguments, string propertyName)
    {
        return arguments[propertyName] as JsonObject
            ?? throw new ArgumentException($"Argument '{propertyName}' is required.");
    }

    public static JsonNode GetRequiredNode(JsonObject arguments, string propertyName)
    {
        return arguments[propertyName]
            ?? throw new ArgumentException($"Argument '{propertyName}' is required.");
    }

    public static object? ToRawValue(JsonNode valueNode)
    {
        return valueNode switch
        {
            JsonValue jsonValue when jsonValue.TryGetValue<bool>(out var boolValue) => boolValue,
            JsonValue jsonValue when jsonValue.TryGetValue<int>(out var intValue) => intValue,
            JsonValue jsonValue when jsonValue.TryGetValue<long>(out var longValue) => longValue,
            JsonValue jsonValue when jsonValue.TryGetValue<double>(out var doubleValue) => doubleValue,
            JsonValue jsonValue when jsonValue.TryGetValue<string>(out var stringValue) => stringValue,
            _ => throw new ArgumentException("Unsupported argument value type.")
        };
    }
}
