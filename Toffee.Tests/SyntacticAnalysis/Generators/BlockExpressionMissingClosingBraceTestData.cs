using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class BlockExpressionMissingClosingBraceTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var leftBraceToken = Helpers.GetDefaultToken(TokenType.LeftBrace);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        // empty
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                semicolonToken
            },
            Array.Empty<Statement>(),
            (null as Statement)!,
            new UnexpectedToken(new Position(1, 1, 1), TokenType.Semicolon, TokenType.RightBrace),
            true  // with no statements there is no semicolon skipping
        };
        // basic with result
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a")
            },
            Array.Empty<Statement>(),
            new ExpressionStatement(new IdentifierExpression("a")),
            new UnexpectedToken(new Position(2, 1, 2), TokenType.Semicolon, TokenType.RightBrace),
            false
        };
        // basic with regular
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                }
            },
            (null as Statement)!,
            new UnexpectedToken(new Position(3, 1, 3), TokenType.Semicolon, TokenType.RightBrace),
            false
        };
        // basic with regular and more semicolons
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                semicolonToken
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                }
            },
            (null as Statement)!,
            new UnexpectedToken(new Position(4, 1, 4), TokenType.Semicolon, TokenType.RightBrace),
            false  // semicolons are skipped as a part of statements block
        };
        // with regular and result
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                new(TokenType.Identifier, "b")
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                }
            },
            new ExpressionStatement(new IdentifierExpression("b")),
            new UnexpectedToken(new Position(4, 1, 4), TokenType.Semicolon, TokenType.RightBrace),
            false
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
            (null as Statement)!,
            new UnexpectedToken(new Position(5, 1, 5), TokenType.Semicolon, TokenType.RightBrace),
            false
        };
        // double regular and more semicolons
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                new(TokenType.Identifier, "b"),
                semicolonToken,
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
            (null as Statement)!,
            new UnexpectedToken(new Position(6, 1, 6), TokenType.Semicolon, TokenType.RightBrace),
            false  // semicolons are skipped as a part of statements block
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
