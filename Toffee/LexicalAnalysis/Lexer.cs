using System.Text;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer : BaseLexer
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

        CurrentToken = new Token(TokenType.Unknown);
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

    private void CollectDigitConsideringOverflowGivenRadix(int radix, ref ulong buffer, ref bool overflowOccurred)
    {
        var digitPosition = _scanner.CurrentPosition;
        if (overflowOccurred)
        {
            _scanner.Advance();
            return;
        }
        try
        {
            buffer = checked((ulong)radix * buffer + (ulong)CharToDigit(_scanner.Advance()!.Value));
        }
        catch (OverflowException)
        {
            overflowOccurred = true;
            EmitError(new NumberLiteralTooLarge(digitPosition));
        }
    }

    private void CollectCharConsideringLengthLimit(StringBuilder buffer, ref bool maxLengthExceeded)
    {
        var charPosition = _scanner.CurrentPosition;
        AppendCharConsideringLengthLimit(buffer, _scanner.Advance(), ref maxLengthExceeded, charPosition);
    }

    private void AppendCharConsideringLengthLimit(StringBuilder buffer, char? c, ref bool maxLengthExceeded, Position charPosition)
    {
        if (!maxLengthExceeded && buffer.Length >= MaxLexemeLength)
        {
            maxLengthExceeded = true;
            EmitError(new ExceededMaxLexemeLength(charPosition, MaxLexemeLength));
        }
        if (maxLengthExceeded)
            return;
        try
        {
            buffer.Append(c);
        }
        catch (ArgumentOutOfRangeException)
        {
            maxLengthExceeded = true;
            EmitError(new ExceededMaxLexemeLength(charPosition, MaxLexemeLength));
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

    private Token FillInTokenPosition(Token baseToken) =>
        baseToken with { StartPosition = _tokenStartPosition, EndPosition = _scanner.CurrentPosition };

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

    public override Token Advance()
    {
        CurrentError = null;
        var supersededToken = CurrentToken;
        SkipWhitespaces();
        _tokenStartPosition = _scanner.CurrentPosition;

        if (_scanner.CurrentCharacter is null)
            CurrentToken = FillInTokenPosition(new Token(TokenType.EndOfText, "ETX"));
        else if (TryMatchToken(out var matchedToken))
            CurrentToken = FillInTokenPosition(matchedToken);
        else
        {
            var unknownTokenPosition = _scanner.CurrentPosition;
            var buffer = $"{_scanner.Advance()}";
            if (char.IsHighSurrogate(buffer[0]))
                buffer += _scanner.Advance();
            EmitError(new UnknownToken(unknownTokenPosition, buffer));
            CurrentToken = FillInTokenPosition(new Token(TokenType.Unknown, buffer));
        }
        return supersededToken;
    }
}
