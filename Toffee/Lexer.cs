using System.Text;

namespace Toffee;

public class Lexer
{
    private readonly Scanner _scanner;

    private Position _tokenStartPosition;
    public Token CurrentToken { get; private set; }

    private delegate Token? MatchDelegate();
    private readonly List<MatchDelegate> _matchers;

    public Lexer(Scanner scanner)
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
        static bool IsCommentOpening(string s) => s is "//" or "/*";

        if (!IsSymbol(_scanner.CurrentCharacter))
            return null;
        var operatorBuilder = new StringBuilder();

        while (IsSymbol(_scanner.CurrentCharacter))
        {
            operatorBuilder.Append(_scanner.CurrentCharacter!.Value);
            _scanner.Advance();
            if (operatorBuilder.Length == 2 && IsCommentOpening(operatorBuilder.ToString()))
                return ContinueMatchingComment(operatorBuilder.ToString() is "/*");
        }

        return OperatorMapper.MapToToken(operatorBuilder.ToString());
    }

    private Token ContinueMatchingComment(bool isBlock)
    {
        var contentBuilder = new StringBuilder();
        if (isBlock)
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
            while (_scanner.CurrentCharacter is not (null or '\n'))
            {
                contentBuilder.Append(_scanner.CurrentCharacter.Value);
                _scanner.Advance();
            }
        }
        return new Token(isBlock ? TokenType.BlockComment : TokenType.LineComment, contentBuilder.ToString());
    }

    private Token? MatchKeywordOrIdentifier()
    {
        if (_scanner.CurrentCharacter is null || !char.IsLetter(_scanner.CurrentCharacter.Value))
            return null;
        var nameBuilder = new StringBuilder($"{_scanner.CurrentCharacter.Value}");
        _scanner.Advance();
        bool IsPartOfIdentifier(char? c) => c is not null && (char.IsLetterOrDigit(c.Value) || c is '_');
        while (IsPartOfIdentifier(_scanner.CurrentCharacter))
        {
            nameBuilder.Append(_scanner.CurrentCharacter.Value);
            _scanner.Advance();
        }
        var name = nameBuilder.ToString();
        return KeywordOrIdentifierMapper.MapToKeywordOrIdentifier(name);
    }

    private Token? MatchNumber()
    {
        // TODO: 0x, 0c, 0b literals
        // TODO: scientific notation
        bool IsDigit(char? c) => c is >= '0' and <= '9';
        long CharToDigit(char c) => c - '0';
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
