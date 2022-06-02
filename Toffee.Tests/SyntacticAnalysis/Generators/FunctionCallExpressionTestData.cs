using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class FunctionCallExpressionTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var commaToken = Helpers.GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                leftParenthesisToken,
                rightParenthesisToken
            },
            new IdentifierExpression("a"),
            Array.Empty<Expression>()
        };
        // with an argument
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                leftParenthesisToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken
            },
            new IdentifierExpression("a"),
            new[]
            {
                new IdentifierExpression("b")
            }
        };
        // with more than one argument
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                leftParenthesisToken,
                new(TokenType.Identifier, "b"),
                commaToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken
            },
            new IdentifierExpression("a"),
            new[]
            {
                new IdentifierExpression("b"),
                new IdentifierExpression("c")
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
