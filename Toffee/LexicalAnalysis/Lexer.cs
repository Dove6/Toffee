using System.Text;
using Toffee.Logging;
using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer : LexerBase
{
    private readonly IScanner _scanner;
    private readonly Logger? _logger;
    private Position _tokenStartPosition;

    private uint CurrentOffset => _scanner.CurrentPosition.Character - _tokenStartPosition.Character;

    private delegate Token? MatchDelegate();
    private readonly List<MatchDelegate> _matchers;

    public Lexer(IScanner scanner, Logger? logger = null, int? maxLexemeLength = null) : base(maxLexemeLength)
    {
        _scanner = scanner;
        _logger = logger;

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

    private void AppendDigitConsideringOverflowGivenRadix(int radix, ref long buffer, char digit, ref bool overflowOccurred, uint? offset = null)
    {
        if (!overflowOccurred)
        {
            try
            {
                buffer = checked(radix * buffer + CharToDigit(digit));
            }
            catch (OverflowException)
            {
                overflowOccurred = true;
                EmitError(new NumberLiteralTooLarge(offset ?? CurrentOffset));
            }
        }
    }

    private void AppendCharConsideringLengthLimit(StringBuilder buffer, char? c, ref bool maxLengthExceeded, uint? offset = null)
    {
        if (maxLengthExceeded)
            return;
        if (buffer.Length >= MaxLexemeLength)
        {
            maxLengthExceeded = true;
            EmitError(new ExceededMaxLexemeLength(offset ?? CurrentOffset));
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
                EmitError(new ExceededMaxLexemeLength(offset ?? CurrentOffset));
            }
        }
    }

    private void EmitError(LexerError error)
    {
        CurrentError = error;
        _logger?.LogError(_tokenStartPosition, error.ToMessage(), error);
    }

    private void EmitWarning(LexerWarning warning)
    {
        _logger?.LogWarning(_tokenStartPosition, warning.ToMessage(), warning);
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
            var buffer = $"{_scanner.CurrentCharacter.Value}";
            if (char.IsHighSurrogate(buffer[0]))
            {
                _scanner.Advance();
                buffer += _scanner.CurrentCharacter;
            }
            EmitError(new UnknownToken());
            CurrentToken = new Token(TokenType.Unknown, buffer, _tokenStartPosition);
        }
    }
}
