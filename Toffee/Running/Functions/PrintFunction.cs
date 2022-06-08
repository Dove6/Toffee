using System.Globalization;

namespace Toffee.Running.Functions;

public class PrintFunction : IFunction
{
    private readonly TextWriter _writer;

    public PrintFunction(TextWriter writer) => _writer = writer;

    public object? Call(IRunner runner, IList<object?> arguments)
    {
        foreach (var argument in arguments)
            _writer.WriteLine(Stringify(argument));
        return null;
    }

    private static string Stringify(object? value)
    {
        return value switch
        {
            null => "null",
            string stringValue => stringValue,
            double floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
            bool boolValue => boolValue ? "true" : "false",
            IFunction => "<function>",
            var other => $"{other}"
        };
    }
}
