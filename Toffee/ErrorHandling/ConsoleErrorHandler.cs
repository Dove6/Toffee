using Toffee.LexicalAnalysis;
using Toffee.Running;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.ErrorHandling;

public class ConsoleErrorHandler : ILexerErrorHandler, IParserErrorHandler, IRunnerErrorHandler
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

    public void Handle(ParserError parserError) =>
        Log(LogLevel.Error, parserError.Position, parserError.ToMessage(), parserError);

    public void Handle(ParserWarning parserWarning) =>
        Log(LogLevel.Warning, parserWarning.Position, parserWarning.ToMessage(), parserWarning);

    public void Handle(RunnerError runnerError) =>
        Log(LogLevel.Error, runnerError.Position, runnerError.ToMessage(), runnerError);

    public void Handle(RunnerWarning runnerWarning) =>
        Log(LogLevel.Warning, runnerWarning.Position, runnerWarning.ToMessage(), runnerWarning);

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
