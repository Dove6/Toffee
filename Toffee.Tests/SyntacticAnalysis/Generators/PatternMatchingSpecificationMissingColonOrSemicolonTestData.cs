using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class PatternMatchingSpecificationMissingColonOrSemicolonTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var matchToken = Helpers.GetDefaultToken(TokenType.KeywordMatch);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var leftBrace = Helpers.GetDefaultToken(TokenType.LeftBrace);
        var rightBrace = Helpers.GetDefaultToken(TokenType.RightBrace);
        var colonToken = Helpers.GetDefaultToken(TokenType.Colon);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var defaultToken = Helpers.GetDefaultToken(TokenType.KeywordDefault);
        // non-default branch, missing colon
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                new(TokenType.Identifier, "c"),
                semicolonToken,
                rightBrace,
                semicolonToken
            },
            new PatternMatchingExpression(new IdentifierExpression("a"),
                new List<PatternMatchingBranch> { new(new IdentifierExpression("b"), new IdentifierExpression("c")) }),
            new UnexpectedToken(new Position(6, 1, 6), TokenType.Identifier, TokenType.Colon)
        };
        // non-default branch, missing semicolon
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                rightBrace,
                semicolonToken
            },
            new PatternMatchingExpression(new IdentifierExpression("a"),
                new List<PatternMatchingBranch> { new(new IdentifierExpression("b"), new IdentifierExpression("c")) }),
            new UnexpectedToken(new Position(8, 1, 8), TokenType.Identifier, TokenType.Semicolon)
        };
        // default branch, missing colon
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                defaultToken,
                new(TokenType.Identifier, "b"),
                semicolonToken,
                rightBrace,
                semicolonToken
            },
            new PatternMatchingExpression(new IdentifierExpression("a"), new List<PatternMatchingBranch>(),
                new IdentifierExpression("b")),
            new UnexpectedToken(new Position(6, 1, 6), TokenType.Identifier, TokenType.Colon)
        };
        // default branch, missing semicolon
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                defaultToken,
                colonToken,
                new(TokenType.Identifier, "b"),
                rightBrace,
                semicolonToken
            },
            new PatternMatchingExpression(new IdentifierExpression("a"), new List<PatternMatchingBranch>(),
                new IdentifierExpression("b")),
            new UnexpectedToken(new Position(8, 1, 8), TokenType.Identifier, TokenType.Semicolon)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
