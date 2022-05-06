using System.Text;
using Toffee.Logging;
using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer : LexerBase
{
    private readonly IScanner _scanner;
    private readonly ILexerErrorHandler? _errorHandler;
    private Position _tokenStartPosition;

    private delegate Token? MatchDelegate();
    private readonly List<MatchDelegate> _matchers;

    public Lexer(IScanner scanner, ILexerErrorHandler? errorHandler = null, int? maxLexemeLength = null) : base(maxLexemeLength)
    {
        _scanner = scanner;
        _errorHandler = errorHandler;

        _matchers = new List<MatchDelegate>
        {
            MatchOperatorOrComment,
            MatchKeywordOrIdentifier,
            MatchNumber,
            MatchString
        };

        Advance();
    }

    private static bool IsDigitGivenRadix(int radix, char? c) => radix switch
    {
        16 => c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f',
        10 => c is >= '0' and <= '9',
        8  => c is >= '0' and <= '7',
        2  => c is '0' or '1',
        _ => throw new ArgumentOutOfRangeException(nameof(radix), radix, "Radix not supported")
    };

    private static int CharToDigit(char c) => c >= 'a'
        ? c - 'a' + 10
        : c >= 'A'
            ? c - 'A' + 10
            : c - '0';

    private void AppendDigitConsideringOverflowGivenRadix(int radix, ref ulong buffer, char digit, ref bool overflowOccurred, Position? errorPosition = null)
    {
        if (overflowOccurred)
            return;
        try
        {
            buffer = checked((ulong)radix * buffer + (ulong)CharToDigit(digit));
        }
        catch (OverflowException)
        {
            overflowOccurred = true;
            EmitError(new NumberLiteralTooLarge(errorPosition ?? _scanner.CurrentPosition));
        }
    }

    private void AppendCharConsideringLengthLimit(StringBuilder buffer, char? c, ref bool maxLengthExceeded, Position? errorPosition = null)
    {
        if (maxLengthExceeded)
            return;
        if (buffer.Length >= MaxLexemeLength)
        {
            maxLengthExceeded = true;
            EmitError(new ExceededMaxLexemeLength(errorPosition ?? _scanner.CurrentPosition, MaxLexemeLength));
        }
        else
        {
            try
            {
                buffer.Append(c);
            }
            catch (ArgumentOutOfRangeException)
            {
                maxLengthExceeded = true;
                EmitError(new ExceededMaxLexemeLength(errorPosition ?? _scanner.CurrentPosition, MaxLexemeLength));
            }
        }
    }

    private void EmitError(LexerError error)
    {
        CurrentError = error;
        _errorHandler?.Handle(error);
    }

    private void EmitWarning(LexerWarning warning)
    {
        _errorHandler?.Handle(warning);
    }

    private bool TryMatchToken(out Token matchedToken)
    {
        matchedToken = new Token(TokenType.Unknown);
        foreach (var matcher in _matchers)
        {
            var matcherResult = matcher();
            if (matcherResult is null)
                continue;
            matchedToken = matcherResult.Value;
            return true;
        }
        return false;
    }

    private void SkipWhitespaces()
    {
        while (_scanner.CurrentCharacter.HasValue && char.IsWhiteSpace(_scanner.CurrentCharacter.Value))
            _scanner.Advance();
    }

    // TODO: return the superseded token
    public override void Advance()
    {
        SkipWhitespaces();

        _tokenStartPosition = _scanner.CurrentPosition;

        if (_scanner.CurrentCharacter is null)
            CurrentToken = new Token(TokenType.EndOfText, "ETX", _tokenStartPosition);
        else if (TryMatchToken(out var matchedToken))
            CurrentToken = matchedToken with { Position = _tokenStartPosition };
        else
        {
            var unknownTokenPosition = _scanner.CurrentPosition;
            var buffer = $"{_scanner.CurrentCharacter.Value}";
            _scanner.Advance();
            if (char.IsHighSurrogate(buffer[0]))
            {
                buffer += _scanner.CurrentCharacter;
                _scanner.Advance();
            }
            EmitError(new UnknownToken(unknownTokenPosition, buffer));
            CurrentToken = new Token(TokenType.Unknown, buffer, _tokenStartPosition);
        }
    }
}
