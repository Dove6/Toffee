using System.Text;
using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public class Lexer : ILexer
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

    private Token? MatchOperatorOrComment()
    {
        static bool IsSymbol(char? c) => c is not (null or '"') && (char.IsSymbol(c.Value) || char.IsPunctuation(c.Value));
        static bool CanExtend(string s, char c) => OperatorMapper.IsTransitionExistent(s, c);

        if (!IsSymbol(_scanner.CurrentCharacter))
            return null;
        var symbolString = "";  // TODO: limit length

        while (IsSymbol(_scanner.CurrentCharacter) && CanExtend(symbolString, _scanner.CurrentCharacter!.Value))
        {
            symbolString += _scanner.CurrentCharacter!.Value;
            _scanner.Advance();
        }

        var resultingToken = OperatorMapper.MapToToken(symbolString);
        if (resultingToken.Type is TokenType.LineComment or TokenType.BlockComment)
            return ContinueMatchingComment(resultingToken.Type);
        if (resultingToken.Type is TokenType.UnknownOperator)
            new string("Unknown token");  // TODO: error

        return resultingToken;
    }

    private Token ContinueMatchingComment(TokenType commentType)
    {
        // TODO: limit length
        var contentBuilder = new StringBuilder();
        if (commentType == TokenType.BlockComment)
        {
            var matchedEnd = false;
            while (_scanner.CurrentCharacter is not null)
            {
                var buffer = _scanner.CurrentCharacter.Value;
                _scanner.Advance();
                if ((buffer, _scanner.CurrentCharacter) is ('*', '/'))
                {
                    matchedEnd = true;
                    _scanner.Advance();
                    break;
                }
                contentBuilder.Append(buffer);
            }
            if (!matchedEnd)
                new string("Unexpected ETX");  // TODO: error
        }
        else
        {
            commentType = TokenType.LineComment;  // just in case
            while (_scanner.CurrentCharacter is not (null or '\n'))
            {
                contentBuilder.Append(_scanner.CurrentCharacter.Value);
                _scanner.Advance();
            }
        }
        return new Token(commentType, contentBuilder.ToString());
    }

    private Token? MatchKeywordOrIdentifier()
    {
        // TODO: limit length
        static bool IsPartOfIdentifier(char? c) => c is not null && (char.IsLetterOrDigit(c.Value) || c is '_');
        if (_scanner.CurrentCharacter is null || !char.IsLetter(_scanner.CurrentCharacter.Value))
            return null;

        var nameBuilder = new StringBuilder($"{_scanner.CurrentCharacter.Value}");
        _scanner.Advance();
        while (IsPartOfIdentifier(_scanner.CurrentCharacter))
        {
            nameBuilder.Append(_scanner.CurrentCharacter.Value);
            _scanner.Advance();
        }
        return KeywordOrIdentifierMapper.MapToKeywordOrIdentifier(nameBuilder.ToString());
    }

    private Token? MatchNumber()
    {
        var radix = 10;

        bool IsDigit(char? c) => radix switch
        {
            16 => c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f',
            8 => c is >= '0' and <= '7',
            2 => c is '0' or '1',
            _ => c is >= '0' and <= '9',
        };
        static long CharToDigit(char c) => c >= 'a'
            ? c - 'a' + 10
            : c >= 'A'
                ? c - 'A' + 10
                : c - '0';

        if (!IsDigit(_scanner.CurrentCharacter))
            return null;
        var integralPart = CharToDigit(_scanner.CurrentCharacter!.Value);
        _scanner.Advance();
        if (integralPart == 0 && _scanner.CurrentCharacter is 'x' or 'c' or 'b')
        {
            radix = _scanner.CurrentCharacter switch
            {
                'x' => 16,
                'c' => 8,
                'b' => 2,
                _   => 10
            };
            _scanner.Advance();
        }
        try
        {
            while (IsDigit(_scanner.CurrentCharacter))
            {
                integralPart = checked(radix * integralPart + CharToDigit(_scanner.CurrentCharacter.Value));
                _scanner.Advance();
            }
        }
        catch (OverflowException)
        {
            // TODO: error
            while (IsDigit(_scanner.CurrentCharacter))
                _scanner.Advance();
            if (_scanner.CurrentCharacter is not '.' || radix != 10)
                return new Token(TokenType.LiteralInteger);
            _scanner.Advance();
            while (IsDigit(_scanner.CurrentCharacter))
                _scanner.Advance();
            return new Token(TokenType.LiteralFloat);
        }
        if (_scanner.CurrentCharacter is not '.' || radix != 10)
            // no fractional part
            return new Token(TokenType.LiteralInteger, integralPart);

        _scanner.Advance();
        var fractionalPart = 0L;
        var fractionalPartLength = 0;
        try
        {
            while (IsDigit(_scanner.CurrentCharacter))
            {
                fractionalPart = checked(radix * fractionalPart + CharToDigit(_scanner.CurrentCharacter.Value));
                fractionalPartLength++;
                _scanner.Advance();
            }
        }
        catch (OverflowException)
        {
            // TODO: error
            while (IsDigit(_scanner.CurrentCharacter))
                _scanner.Advance();
            return new Token(TokenType.LiteralFloat);
        }
        if (_scanner.CurrentCharacter is not ('e' or 'E'))
            // no exponential part
            return new Token(TokenType.LiteralFloat, integralPart + fractionalPart / Math.Pow(radix, fractionalPartLength));

        _scanner.Advance();
        var exponentialPart = 0L;
        var exponentSign = _scanner.CurrentCharacter is '-' ? -1 : 1;
        if (_scanner.CurrentCharacter is '-' or '+')
            _scanner.Advance();
        while (IsDigit(_scanner.CurrentCharacter))
        {
            exponentialPart = checked(radix * exponentialPart + CharToDigit(_scanner.CurrentCharacter.Value));
            _scanner.Advance();
        }
        var exponentiatedNumber = integralPart * Math.Pow(10, exponentSign * exponentialPart)
            + fractionalPart * Math.Pow(10, exponentSign * exponentialPart - fractionalPartLength);
        return new Token(TokenType.LiteralFloat, exponentiatedNumber);
    }

    private Token? MatchString()
    {
        // TODO: escaping
        // TODO: limit length
        if (_scanner.CurrentCharacter is not '"')
            return null;
        _scanner.Advance();
        var contentBuilder = new StringBuilder();
        while (_scanner.CurrentCharacter is not (null or '"'))
        {
            contentBuilder.Append(_scanner.CurrentCharacter.Value);
            _scanner.Advance();
        }
        if (_scanner.CurrentCharacter is '"')
            _scanner.Advance();
        else
            new string("Unexpected ETX");  // TODO: error
        return new Token(TokenType.LiteralString, contentBuilder.ToString());
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
