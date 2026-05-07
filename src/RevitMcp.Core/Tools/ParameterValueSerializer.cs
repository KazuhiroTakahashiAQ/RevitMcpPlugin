namespace RevitMcp.Core.Tools;

internal static class ParameterValueSerializer
{
    public static object? Serialize(object? value)
    {
        return value switch
        {
            null => null,
            string stringValue => stringValue,
            bool boolValue => boolValue,
            int intValue => intValue,
            long longValue => longValue,
            float floatValue => floatValue,
            double doubleValue => doubleValue,
            decimal decimalValue => decimalValue,
            _ => value.ToString()
        };
    }
}
