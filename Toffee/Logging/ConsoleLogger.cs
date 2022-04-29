using Toffee.Logging;
using Toffee.Scanning;

public class ConsoleLogger : Logger
{
    public ConsoleLogger(string? sourceName = null) : base(sourceName)
    { }

    public override void Log(LogLevel level, Position position, string message, params object?[] attachments)
    {
        var (character, line, column) = position;
        Console.WriteLine($"{level.ToString().ToUpper()} | {SourceName ?? "input"}:{line}:{column} ({character}) | {message}");
        foreach (var attachment in attachments)
            Console.WriteLine($"\t{attachment ?? "null"}");
    }
}
