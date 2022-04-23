using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public partial class Lexer : ILexer
{
    private readonly IScanner _scanner;

    private Position _tokenStartPosition;
    public Token CurrentToken { get; private set; }

    private delegate Token? MatchDelegate();
    private readonly List<MatchDelegate> _matchers;

    public Lexer(IScanner scanner)
    {
        _scanner = scanner;

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

    private static (long, OverflowException?) AppendDigitGivenRadix(int radix, long buffer, char digit)
    {
        try
        {
            return (checked(radix * buffer + CharToDigit(digit)), null);
        }
        catch (OverflowException e)
        {
            return (buffer, e);
        }
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

    public void Advance()
    {
        _tokenStartPosition = _scanner.CurrentPosition;

        SkipWhitespaces();

        if (_scanner.CurrentCharacter is null)
            CurrentToken = new Token(TokenType.EndOfText, "ETX", _tokenStartPosition);
        else if (TryMatchToken(out var matchedToken))
            CurrentToken = matchedToken with { Position = _tokenStartPosition };
        else  // TODO: error
            CurrentToken = new Token(TokenType.Unknown, _scanner.CurrentCharacter, _tokenStartPosition);
    }
}
