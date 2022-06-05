using System.Globalization;
using Toffee.ErrorHandling;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner : IRunner
{
    private readonly IRunnerErrorHandler? _errorHandler;

    public Runner(IRunnerErrorHandler? errorHandler = null)
    {
        _errorHandler = errorHandler;
    }

    private void EmitError(RunnerError error) => _errorHandler?.Handle(error);

    private void EmitWarning(RunnerWarning warning) => _errorHandler?.Handle(warning);

    private static object? CastToNumber(object? value)
    {
        if (value is null)
            return null;

        return value switch
        {
            long integerValue => integerValue,
            double floatValue => floatValue,
            string stringValue => CastToNumber(stringValue),
            bool boolValue => boolValue ? 1L : 0L,
            _ => throw new NotImplementedException()
        };
    }

    private static object? CastToNumber(string value)
    {
        // TODO: mimic lexer implementation
        if (long.TryParse(value, out var integerValue))
            return integerValue;
        if (double.TryParse(value, out var floatValue))
            return floatValue;
        return null;
    }

    private static object? Cast(object? value, DataType type)
    {
        if (value is null)
            return null;

        return type switch
        {
            DataType.Integer => CastToInteger(value),
            DataType.Float => CastToFloat(value),
            DataType.String => CastToString(value),
            DataType.Bool => CastToBool(value),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  // TODO: throws
        };
    }

    private static object? CastToInteger(object value)
    {
        return value switch
        {
            long integerValue => integerValue,
            double floatValue => (long)Math.Truncate(floatValue),
            bool boolValue => boolValue ? 1L : 0L,
            string stringValue => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)  // TODO: throws
        };
    }

    private static object? CastToFloat(object value)
    {
        return value switch
        {
            long integerValue => (double)integerValue,
            double floatValue => floatValue,
            bool boolValue => boolValue ? 1.0 : 0.0,
            string stringValue => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)  // TODO: throws
        };
    }

    private static object? CastToString(object value)
    {
        return value switch
        {
            long integerValue => integerValue.ToString(CultureInfo.InvariantCulture),
            double floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
            bool boolValue => boolValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
            string stringValue => stringValue,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)  // TODO: throws
        };
    }

    private static object? CastToBool(object value)
    {
        return value switch
        {
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
}
