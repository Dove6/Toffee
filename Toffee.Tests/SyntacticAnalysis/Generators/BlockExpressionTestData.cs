using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class BlockExpressionTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var leftBraceToken = Helpers.GetDefaultToken(TokenType.LeftBrace);
        var rightBraceToken = Helpers.GetDefaultToken(TokenType.RightBrace);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        // basic unterminated
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                rightBraceToken
            },
            Array.Empty<Statement>(),
            new ExpressionStatement(new IdentifierExpression("a"))
        };
        // basic terminated
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                rightBraceToken
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                }
            },
            (null as Statement)!
        };
        // terminated and unterminated
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                new(TokenType.Identifier, "b"),
                rightBraceToken
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                }
            },
            new ExpressionStatement(new IdentifierExpression("b"))
        };
        // double terminated
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                new(TokenType.Identifier, "b"),
                semicolonToken,
                rightBraceToken
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                },
                new ExpressionStatement(new IdentifierExpression("b"))
                {
                    IsTerminated = true
                }
            },
            (null as Statement)!
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
