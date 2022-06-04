using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class WhileLoopExpressionTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var whileToken = Helpers.GetDefaultToken(TokenType.KeywordWhile);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        // basic
        yield return new object[]
        {
            new[]
            {
                whileToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new IdentifierExpression("a"),
            new IdentifierExpression("b")
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
