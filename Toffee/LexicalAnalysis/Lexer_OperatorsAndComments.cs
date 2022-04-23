using System.Text;

namespace Toffee.LexicalAnalysis;

public partial class Lexer
{
    private Token? MatchOperatorOrComment()
    {
        static bool IsSymbol(char? c) =>
            c is not (null or '"') && (char.IsSymbol(c.Value) || char.IsPunctuation(c.Value));

        static bool CanExtend(string s, char c) => OperatorMapper.IsTransitionExistent(s, c);

        if (!IsSymbol(_scanner.CurrentCharacter))
            return null;
        var symbolString = ""; // TODO: limit length

        while (IsSymbol(_scanner.CurrentCharacter) && CanExtend(symbolString, _scanner.CurrentCharacter!.Value))
        {
            symbolString += _scanner.CurrentCharacter!.Value;
            _scanner.Advance();
        }

        var resultingToken = OperatorMapper.MapToToken(symbolString);
        if (resultingToken.Type is TokenType.LineComment or TokenType.BlockComment)
            return ContinueMatchingComment(resultingToken.Type);
        if (resultingToken.Type is TokenType.UnknownOperator)
            new string("Unknown token"); // TODO: error

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
                new string("Unexpected ETX"); // TODO: error
        }
        else
        {
            commentType = TokenType.LineComment; // just in case
            while (_scanner.CurrentCharacter is not (null or '\n'))
            {
                contentBuilder.Append(_scanner.CurrentCharacter.Value);
                _scanner.Advance();
            }
        }
        return new Token(commentType, contentBuilder.ToString());
    }
}
