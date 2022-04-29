using System.Text;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer
{
    private Token? MatchOperatorOrComment()
    {
        static bool IsSymbol(char? c) =>
            c is not (null or '"') && (char.IsSymbol(c.Value) || char.IsPunctuation(c.Value));
        static bool CanExtend(string s, char c) => OperatorMapper.IsTransitionExistent(s, c);

        if (!IsSymbol(_scanner.CurrentCharacter) || !CanExtend("", _scanner.CurrentCharacter!.Value))
            return null;
        var symbolString = "";

        while (IsSymbol(_scanner.CurrentCharacter) && CanExtend(symbolString, _scanner.CurrentCharacter!.Value))
        {
            symbolString += _scanner.CurrentCharacter!.Value;
            _scanner.Advance();
        }

        var resultingToken = OperatorMapper.MapToToken(symbolString);
        if (resultingToken.Type is TokenType.Unknown)
            EmitError(new UnknownToken());
        return resultingToken.Type switch
        {
            TokenType.LineComment  => ContinueMatchingLineComment(),
            TokenType.BlockComment => ContinueMatchingBlockComment(),
            _                      => resultingToken
        };
    }

    private Token ContinueMatchingBlockComment()
    {
        var contentBuilder = new StringBuilder();
        var maxLengthExceeded = false;
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
            AppendCharConsideringLengthLimit(contentBuilder, buffer, ref maxLengthExceeded, CurrentOffset - 1);
        }
        if (!matchedEnd)
            EmitError(new UnexpectedEndOfText(CurrentOffset));
        return new Token(TokenType.BlockComment, contentBuilder.ToString());
    }

    private Token ContinueMatchingLineComment()
    {
        var contentBuilder = new StringBuilder();
        var maxLengthExceeded = false;
        while (_scanner.CurrentCharacter is not (null or '\n'))
        {
            AppendCharConsideringLengthLimit(contentBuilder, _scanner.CurrentCharacter, ref maxLengthExceeded);
            _scanner.Advance();
        }
        return new Token(TokenType.LineComment, contentBuilder.ToString());
    }
}
