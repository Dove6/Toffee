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
        static bool IsSymbol(char? c) => c is not null && char.IsSymbol(c.Value);
        static bool CanExtend(string s, char c) => OperatorMapper.IsTransitionExistent(s, c);

        if (!IsSymbol(_scanner.CurrentCharacter))
            return null;
        var symbolString = "";

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
        // TODO: 0x, 0c, 0b literals
        // TODO: scientific notation
        static bool IsDigit(char? c) => c is >= '0' and <= '9';
        static long CharToDigit(char c) => c - '0';

        if (!IsDigit(_scanner.CurrentCharacter))
            return null;
        var integralPart = CharToDigit(_scanner.CurrentCharacter!.Value);
        _scanner.Advance();
        try
        {
            while (IsDigit(_scanner.CurrentCharacter))
            {
                integralPart = checked(10 * integralPart + CharToDigit(_scanner.CurrentCharacter.Value));
                _scanner.Advance();
            }
        }
        catch (OverflowException e)
        {
            // TODO: error
            while (IsDigit(_scanner.CurrentCharacter))
                _scanner.Advance();
            if (_scanner.CurrentCharacter is not '.')
                return new Token { Type = TokenType.LiteralInteger };
            _scanner.Advance();
            while (IsDigit(_scanner.CurrentCharacter))
                _scanner.Advance();
            return new Token { Type = TokenType.LiteralFloat };
        }
        if (_scanner.CurrentCharacter is not '.')
            // no fractional part
            return new Token { Type = TokenType.LiteralInteger, Content = integralPart };

        _scanner.Advance();
        var fractionalPart = 0L;
        var fractionalPartLength = 0;
        try
        {
            while (IsDigit(_scanner.CurrentCharacter))
            {
                fractionalPart = checked(10 * fractionalPart + CharToDigit(_scanner.CurrentCharacter.Value));
                fractionalPartLength++;
                _scanner.Advance();
            }
        }
        catch (OverflowException e)
        {
            // TODO: error
            while (IsDigit(_scanner.CurrentCharacter))
                _scanner.Advance();
            return new Token { Type = TokenType.LiteralFloat };
        }
        var joinedNumber = integralPart + fractionalPart / Math.Pow(10, fractionalPartLength);
        return new Token { Type = TokenType.LiteralFloat, Content = joinedNumber };
    }

    private Token? MatchString()
    {
        // TODO: escaping
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
        return new Token { Type = TokenType.LiteralString, Content = contentBuilder.ToString() };
    }

    private bool TryMatchToken(out Token matchedToken)
    {
        matchedToken = new Token { Type = TokenType.Unknown };
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
            CurrentToken = new Token { Type = TokenType.EndOfText, Content = "ETX", Position = _tokenStartPosition };
        else if (TryMatchToken(out var matchedToken))
            CurrentToken = matchedToken with { Position = _tokenStartPosition };
        else  // TODO: error
            CurrentToken = new Token
                { Type = TokenType.Unknown, Content = _scanner.CurrentCharacter, Position = _tokenStartPosition };
    }
}
