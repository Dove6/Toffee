using CommandDotNet;
using CommandDotNet.Rendering;
using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;
using Toffee.Running;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.CommandLine;

public class Application
{
    private string? _sourceName;
    private TextReader? _reader;
    private IScanner? _scanner;
    private ConsoleErrorHandler? _logger;
    private ILexer? _lexer;
    private IParser? _parser;

    [DefaultCommand]
    public void Execute(
        IConsole console,
        [Named(Description = "Length limit for strings, identifiers and comments")]
        int? maxLexemeLength,
        [Positional(Description = "Path to interpreter input")]
        FileInfo? scriptFilename)
    {
        var isFile = scriptFilename is not null;
        var isInteractiveConsole = !(isFile || console.IsInputRedirected);
        _reader = isFile
            ? new StreamReader(scriptFilename!.Name)
            : console.In;
        _sourceName = scriptFilename?.Name ?? "STDIN";
        _scanner = new Scanner(_reader);
        _logger = new ConsoleErrorHandler(_sourceName, console.Error);
        _lexer = new Lexer(_scanner, _logger, maxLexemeLength);
        _parser = new Parser(_lexer, _logger);
        RunRunner(isInteractiveConsole, console);
    }

    private void RunLexer(IStandardOut console)
    {
        static string FormatPosition(Position position) =>
            $"[{position.Character}] {position.Line}:{position.Column}";
        while (_lexer!.CurrentToken.Type != TokenType.EndOfText)
        {
            var positionDescription = FormatPosition(_lexer.CurrentToken.StartPosition);
            var contentDescription = _lexer.CurrentToken.Content switch
            {
                char charContent => $"0x{Convert.ToByte(charContent):x2}",
                null => "null",
                _ => _lexer.CurrentToken.Content.ToString()
            };
            console.Out.WriteLine(
                $"type: {_lexer.CurrentToken.Type.ToString()}, " +
                $"content: {contentDescription}, " +
                $"position: {positionDescription}");
            _lexer.Advance();
        }
    }

    private void RunAstPrinter(IStandardOut console)
    {
        var printer = new AstPrinter(_sourceName!, console.Out);
        while (_parser!.TryAdvance(out var statement) && statement is not null)
            printer.Print(statement);
    }

    private void RunRunner(bool isInteractive, IStandardOut console)
    {
        var runner = new Runner(_logger, writer: console.Out);

        bool successfullyParsed;
        while (!runner.ShouldQuit)
        {
            successfullyParsed = _parser!.TryAdvance(out var statement);
            if (successfullyParsed && statement is null)
                return;
            if (!successfullyParsed && !isInteractive)
                break;
            if (successfullyParsed)
                runner.Run(statement!);
        }

        if (isInteractive || runner.ShouldQuit)
            return;

        while (!_parser!.TryAdvance(out var statement) || statement is not null)
        { }
    }
}
