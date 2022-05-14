﻿using CommandDotNet;
using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;
using Toffee.Running;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.CommandLine;

public class Application
{
    private TextReader? _reader;
    private IScanner? _scanner;
    private ILexerErrorHandler? _logger;
    private ILexer? _lexer;
    private IParser? _parser;

    [DefaultCommand]
    public void Execute(
        [Named(Description = "Length limit for strings, identifiers and comments")]
        int? maxLexemeLength,
        [Positional(Description = "Path to interpreter input")]
        FileInfo? scriptFilename)
    {
        _reader = scriptFilename is null
            ? Console.In
            : new StreamReader(scriptFilename.Name);
        var sourceName = scriptFilename?.Name ?? "STDIN";
        _scanner = new Scanner(_reader);
        _logger = new ConsoleErrorHandler(sourceName);
        _lexer = new Lexer(_scanner, _logger, maxLexemeLength);
        _parser = new Parser(_lexer);
        RunParser();
    }

    private void RunLexer()
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
            Console.WriteLine(
                $"type: {_lexer.CurrentToken.Type.ToString()}, " +
                $"content: {contentDescription}, " +
                $"position: {positionDescription}");
            _lexer.Advance();
        }
    }

    private void RunParser()
    {
        while (_parser!.CurrentStatement is not null)
        {
            AstPrinter.Print(_parser.CurrentStatement);
            _parser.Advance();
        }
    }
}
