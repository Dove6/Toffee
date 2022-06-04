using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class BlockExpressionMissingSemicolonTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var leftBraceToken = Helpers.GetDefaultToken(TokenType.LeftBrace);
        var rightBraceToken = Helpers.GetDefaultToken(TokenType.RightBrace);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        // missing semicolon and result
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                new(TokenType.Identifier, "b"),
                rightBraceToken,
                semicolonToken
            },
            new BlockExpression(new List<Statement> { new ExpressionStatement(new IdentifierExpression("a")) },
                new ExpressionStatement(new IdentifierExpression("b"))),
            new ExpectedSemicolon(new Position(2, 1, 2), TokenType.Identifier)
        };
        // missing semicolon and regular
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                new(TokenType.Identifier, "b"),
                semicolonToken,
                rightBraceToken,
                semicolonToken
            },
            new BlockExpression(new List<Statement>
                {
                    new ExpressionStatement(new IdentifierExpression("a")),
                    new ExpressionStatement(new IdentifierExpression("b")) { IsTerminated = true }
                }),
            new ExpectedSemicolon(new Position(2, 1, 2), TokenType.Identifier)
        };
        // missing semicolon and regular and result
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                new(TokenType.Identifier, "b"),
                semicolonToken,
                new(TokenType.Identifier, "c"),
                rightBraceToken,
                semicolonToken
            },
            new BlockExpression(new List<Statement>
                {
                    new ExpressionStatement(new IdentifierExpression("a")),
                    new ExpressionStatement(new IdentifierExpression("b")) { IsTerminated = true }
                },
                new ExpressionStatement(new IdentifierExpression("c"))),
            new ExpectedSemicolon(new Position(2, 1, 2), TokenType.Identifier)
        };
        // double missing semicolon and double regular
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                new(TokenType.Identifier, "b"),
                new(TokenType.Identifier, "c"),
                semicolonToken,
                rightBraceToken,
                semicolonToken
            },
            new BlockExpression(new List<Statement>
                {
                    new ExpressionStatement(new IdentifierExpression("a")),
                    new ExpressionStatement(new IdentifierExpression("b")),
                    new ExpressionStatement(new IdentifierExpression("c")) { IsTerminated = true }
                }),
            new ExpectedSemicolon(new Position(2, 1, 2), TokenType.Identifier),
            new ExpectedSemicolon(new Position(3, 1, 3), TokenType.Identifier)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
