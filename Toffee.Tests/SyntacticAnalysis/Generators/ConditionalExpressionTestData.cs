using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class ConditionalExpressionTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var ifToken = Helpers.GetDefaultToken(TokenType.KeywordIf);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var elifToken = Helpers.GetDefaultToken(TokenType.KeywordElif);
        var elseToken = Helpers.GetDefaultToken(TokenType.KeywordElse);
        // basic
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            Array.Empty<ConditionalElement>(),
            (null as Expression)!
        };
        // with else
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                elseToken,
                new(TokenType.Identifier, "c")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            Array.Empty<ConditionalElement>(),
            new IdentifierExpression("c")
        };
        // with elif
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                elifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            new[]
            {
                new ConditionalElement(new IdentifierExpression("c"), new IdentifierExpression("d"))
            },
            (null as Expression)!
        };
        // with more than one elif
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                elifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d"),
                elifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "e"),
                rightParenthesisToken,
                new(TokenType.Identifier, "f")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            new[]
            {
                new ConditionalElement(new IdentifierExpression("c"), new IdentifierExpression("d")),
                new ConditionalElement(new IdentifierExpression("e"), new IdentifierExpression("f"))
            },
            (null as Expression)!
        };
        // with elif and else
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                elifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d"),
                elseToken,
                new(TokenType.Identifier, "e")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            new[]
            {
                new ConditionalElement(new IdentifierExpression("c"), new IdentifierExpression("d"))
            },
            new IdentifierExpression("e")
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
