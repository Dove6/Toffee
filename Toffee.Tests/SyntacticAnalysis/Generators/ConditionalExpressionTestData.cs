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
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
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
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
            {
                new(new IdentifierExpression("a"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("b")))
            })
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
                new(TokenType.Identifier, "c"),
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
                {
                    new(new IdentifierExpression("a"),
                        new BlockExpression(new List<Statement>(), new IdentifierExpression("b")))
                },
                new BlockExpression(new List<Statement>(), new IdentifierExpression("c")))
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
                new(TokenType.Identifier, "d"),
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
            {
                new(new IdentifierExpression("a"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("b"))),
                new(new IdentifierExpression("c"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("d")))
            })
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
                new(TokenType.Identifier, "f"),
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
            {
                new(new IdentifierExpression("a"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("b"))),
                new(new IdentifierExpression("c"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("d"))),
                new(new IdentifierExpression("e"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("f")))
            })
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
                new(TokenType.Identifier, "e"),
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
                {
                    new(new IdentifierExpression("a"),
                        new BlockExpression(new List<Statement>(), new IdentifierExpression("b"))),
                    new(new IdentifierExpression("c"),
                        new BlockExpression(new List<Statement>(), new IdentifierExpression("d")))
                },
                new BlockExpression(new List<Statement>(), new IdentifierExpression("e")))
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
