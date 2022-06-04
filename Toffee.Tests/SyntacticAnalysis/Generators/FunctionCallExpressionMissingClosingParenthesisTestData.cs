using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class FunctionCallExpressionMissingClosingParenthesisTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var commaToken = Helpers.GetDefaultToken(TokenType.Comma);
        // no arguments
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                leftParenthesisToken,
                semicolonToken
            },
            new FunctionCallExpression(new IdentifierExpression("a"), new List<Expression>()),
            new UnexpectedToken(new Position(2, 1, 2), TokenType.Semicolon, TokenType.RightParenthesis)
        };
        // an argument
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                leftParenthesisToken,
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new FunctionCallExpression(new IdentifierExpression("a"), new List<Expression> { new IdentifierExpression("b") }),
            new UnexpectedToken(new Position(3, 1, 3), TokenType.Semicolon, TokenType.RightParenthesis)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
