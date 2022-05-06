using Toffee.LexicalAnalysis;
using Toffee.Scanning;

namespace Toffee.Logging;

public class ConsoleErrorHandler : ILexerErrorHandler
{
    private readonly string? _sourceName;

    public ConsoleErrorHandler(string? sourceName = null)
    {
        _sourceName = sourceName;
    }

    public void Handle(LexerError lexerError) =>
        Log(LogLevel.Error, lexerError.Position, lexerError.ToMessage(), lexerError);

    public void Handle(LexerWarning lexerWarning) =>
        Log(LogLevel.Warning, lexerWarning.Position, lexerWarning.ToMessage(), lexerWarning);

    private void Log(LogLevel level, Position position, string message, params object?[] attachments)
    {
        var (character, line, column) = position;
        Console.WriteLine($"{level.ToString().ToUpper()} | {_sourceName ?? "input"}:{line}:{column} ({character}) | {message}");
        foreach (var attachment in attachments)
            Console.WriteLine($"\t{attachment ?? "null"}");
    }

    private enum LogLevel
    {
        Error,
        Warning
    }
}
