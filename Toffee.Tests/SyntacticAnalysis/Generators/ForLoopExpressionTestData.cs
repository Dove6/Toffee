using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class ForLoopExpressionTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var forToken = Helpers.GetDefaultToken(TokenType.KeywordFor);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var colonToken = Helpers.GetDefaultToken(TokenType.Colon);
        var comma = Helpers.GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b")
            },
            null!,
            new ForLoopRange(new IdentifierExpression("a")),
            new IdentifierExpression("b")
        };
        // with start:stop range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                colonToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                new(TokenType.Identifier, "c")
            },
            null!,
            new ForLoopRange(Start: new IdentifierExpression("a"), PastTheEnd: new IdentifierExpression("b")),
            new IdentifierExpression("c")
        };
        // with start:stop:step range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                colonToken,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d")
            },
            null!,
            new ForLoopRange(Start: new IdentifierExpression("a"), PastTheEnd: new IdentifierExpression("b"), Step: new IdentifierExpression("c")),
            new IdentifierExpression("d")
        };
        // with counter
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                comma,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                new(TokenType.Identifier, "c")
            },
            "a",
            new ForLoopRange(new IdentifierExpression("b")),
            new IdentifierExpression("c")
        };
        // with counter and start:stop range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                comma,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d")
            },
            "a",
            new ForLoopRange(Start: new IdentifierExpression("b"), PastTheEnd: new IdentifierExpression("c")),
            new IdentifierExpression("d")
        };
        // with counter and start:stop:step range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                comma,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                colonToken,
                new(TokenType.Identifier, "d"),
                rightParenthesisToken,
                new(TokenType.Identifier, "e")
            },
            "a",
            new ForLoopRange(Start: new IdentifierExpression("b"), PastTheEnd: new IdentifierExpression("c"), Step: new IdentifierExpression("d")),
            new IdentifierExpression("e")
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
