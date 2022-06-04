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
        // empty
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                rightBraceToken,
                semicolonToken
            },
            Array.Empty<Statement>(),
            (null as Statement)!
        };
        // basic with result
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                rightBraceToken,
                semicolonToken
            },
            Array.Empty<Statement>(),
            new ExpressionStatement(new IdentifierExpression("a"))
        };
        // basic with regular
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                rightBraceToken,
                semicolonToken
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
        // with regular and result
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                new(TokenType.Identifier, "b"),
                rightBraceToken,
                semicolonToken
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
        // double regular
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                new(TokenType.Identifier, "b"),
                semicolonToken,
                rightBraceToken,
                semicolonToken
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
