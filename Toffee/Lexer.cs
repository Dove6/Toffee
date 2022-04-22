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
            MatchKeywordOrIdentifier,
            MatchComment,
            MatchSymbol,
            MatchNumber,
            MatchString
        };

        Advance();
    }

    private Token? MatchKeywordOrIdentifier()
    {
        if (!_scanner.CurrentCharacter.HasValue)
            return null;
        if (!char.IsLetter(_scanner.CurrentCharacter.Value))
            return null;
        var nameBuilder = new StringBuilder($"{_scanner.CurrentCharacter.Value}");
        bool IsPartOfIdentifier(char c) => char.IsLetterOrDigit(c) || c == '_';
        while (_scanner.NextCharacter is not null && IsPartOfIdentifier(_scanner.NextCharacter.Value))
        {
            _scanner.Advance();
            nameBuilder.Append(_scanner.CurrentCharacter.Value);
        }
        var name = nameBuilder.ToString();
        return new Token(KeywordMapper.TellKeywordFromIdentifier(name), name);
    }

    private Token? MatchSymbol()
    {
        var matchedToken = (_scanner.CurrentCharacter, _scanner.NextCharacter) switch
        {
            ('+', _)   => new Token(TokenType.OperatorPlus, "+"),
            ('-', _)   => new Token(TokenType.OperatorMinus, "-"),
            ('*', _)   => new Token(TokenType.OperatorAsterisk, "*"),
            ('/', _)   => new Token(TokenType.OperatorSlash, "/"),
            ('!', '=') => new Token(TokenType.OperatorBangEqual, "!="),
            ('!', _)   => new Token(TokenType.OperatorBang, "!"),
            ('=', '=') => new Token(TokenType.OperatorEqualEqual, "=="),
            ('=', _)   => new Token(TokenType.OperatorEqual, "="),
            ('<', '=') => new Token(TokenType.OperatorLessEqual, "<="),
            ('<', _)   => new Token(TokenType.OperatorLess, "<"),
            ('>', '=') => new Token(TokenType.OperatorGreaterEqual, ">="),
            ('>', _)   => new Token(TokenType.OperatorGreater, ">"),
            ('.', _)   => new Token(TokenType.OperatorDot, "."),
            (',', _)   => new Token(TokenType.OperatorComma, ","),
            (';', _)   => new Token(TokenType.Semicolon, ";"),
            ('(', _)   => new Token(TokenType.OpeningParenthesis, "("),
            (')', _)   => new Token(TokenType.ClosingParenthesis, ")"),
            ('[', _)   => new Token(TokenType.OpeningBracket, "["),
            (']', _)   => new Token(TokenType.ClosingBracket, "]"),
            ('{', _)   => new Token(TokenType.OpeningBrace, "{"),
            ('}', _)   => new Token(TokenType.ClosingBrace, "}"),
            (_, _)     => (Token?)null
        };
        for (var i = 1; i < (matchedToken?.Content as string)?.Length; i++)
            _scanner.Advance();
        return matchedToken;
    }

    private Token? MatchComment()
    {
        if ((_scanner.CurrentCharacter, _scanner.NextCharacter) is not (('/', '/') or ('/', '*')))
            return null;
        var isBlock = _scanner.NextCharacter == '*';
        _scanner.Advance();
        var contentBuilder = new StringBuilder();
        if (isBlock)
        {
            var matchedEnd = false;
            while (_scanner.NextCharacter is not null)
            {
                _scanner.Advance();
                if ((_scanner.CurrentCharacter, _scanner.NextCharacter) is ('*', '/'))
                {
                    matchedEnd = true;
                    _scanner.Advance();
                    break;
                }
                contentBuilder.Append(_scanner.CurrentCharacter.Value);
            }
            if (!matchedEnd)
                _logger.LogError(_scanner.CurrentPosition, "Unexpected end of input");
        }
        else
        {
            while (_scanner.NextCharacter is not (null or '\n'))
            {
                _scanner.Advance();
                contentBuilder.Append(_scanner.CurrentCharacter.Value);
            }
        }
        return new Token { Type = TokenType.Comment, Content = contentBuilder.ToString() };
    }

    private Token? MatchNumber()
    {
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
