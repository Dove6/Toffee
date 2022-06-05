using System.Globalization;
using Toffee.Running.Functions;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running.Operations;

public static class Casting
{
    public static object? ToNumber(object? value)
    {
        return value switch
        {
            null => null,
            long integerValue => integerValue,
            double floatValue => floatValue,
            string stringValue => ToNumber(stringValue),
            bool boolValue => boolValue ? 1L : 0L,
            _ => throw new NotImplementedException()
        };
    }

    public static object? ToNumber(string value)
    {
        // TODO: mimic lexer implementation
        if (long.TryParse(value, out var integerValue))
            return integerValue;
        if (double.TryParse(value, out var floatValue))
            return floatValue;
        return null;
    }

    public static object? To(object? value, DataType type)
    {
        return type switch
        {
            DataType.Integer => ToInt(value),
            DataType.Float => ToFloat(value),
            DataType.String => ToString(value),
            DataType.Bool => ToBool(value),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  // TODO: throws
        };
    }

    public static long? ToInt(object? value)
    {
        return value switch
        {
            null => null,
            long integerValue => integerValue,
            double floatValue => (long)Math.Truncate(floatValue),
            bool boolValue => boolValue ? 1L : 0L,
            string stringValue => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)  // TODO: throws
        };
    }

    public static double? ToFloat(object? value)
    {
        return value switch
        {
            null => null,
            long integerValue => Convert.ToDouble(integerValue),
            double floatValue => floatValue,
            bool boolValue => boolValue ? 1.0 : 0.0,
            string stringValue => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)  // TODO: throws
        };
    }

    public static string? ToString(object? value)
    {
        return value switch
        {
            null => null,
            long integerValue => integerValue.ToString(CultureInfo.InvariantCulture),
            double floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
            bool boolValue => boolValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
            string stringValue => stringValue,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)  // TODO: throws
        };
    }

    public static bool? ToBool(object? value)
    {
        return value switch
        {
            null => null,
            long integerValue => integerValue != 0L,
            double floatValue => floatValue != 0L,
            bool boolValue => boolValue,
            string stringValue => stringValue switch
            {
                "true" => true,
                "false" => false,
                _ => null
            },
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)  // TODO: throws
        };
    }

    public static IFunction? ToFunction(object? value)
    {
        return value switch
        {
            IFunction functionValue => functionValue,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)  // TODO: throws
        };
    }
}
