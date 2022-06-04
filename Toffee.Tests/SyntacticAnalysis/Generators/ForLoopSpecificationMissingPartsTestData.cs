using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class ForLoopSpecificationMissingPartsTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var forToken = Helpers.GetDefaultToken(TokenType.KeywordFor);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var colonToken = Helpers.GetDefaultToken(TokenType.Colon);
        var comma = Helpers.GetDefaultToken(TokenType.Comma);
        // missing range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                rightParenthesisToken,
                new(TokenType.Identifier, "a"),
                semicolonToken
            },
            new ExpectedExpression(new Position(2, 1, 2), TokenType.RightParenthesis)
        };
        // missing counter
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                comma,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new ExpectedExpression(new Position(2, 1, 2), TokenType.Comma)
        };
        // start:stop, missing range start
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                colonToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                new(TokenType.Identifier, "c"),
                semicolonToken
            },
            new ExpectedExpression(new Position(2, 1, 2), TokenType.Colon)
        };
        // start:stop, missing range stop
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
                rightParenthesisToken,
                new(TokenType.Identifier, "c"),
                semicolonToken
            },
            new ExpectedExpression(new Position(6, 1, 6), TokenType.RightParenthesis)
        };
        // start:stop:step, missing range start
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                colonToken,
                new(TokenType.Identifier, "a"),
                colonToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                new(TokenType.Identifier, "c"),
                semicolonToken
            },
            new ExpectedExpression(new Position(2, 1, 2), TokenType.Colon)
        };
        // start:stop:step, missing range stop
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                colonToken,
                colonToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                new(TokenType.Identifier, "c"),
                semicolonToken
            },
            new ExpectedExpression(new Position(4, 1, 4), TokenType.Colon)
        };
        // start:stop:step, missing range step
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
                rightParenthesisToken,
                new(TokenType.Identifier, "c"),
                semicolonToken
            },
            new ExpectedExpression(new Position(6, 1, 6), TokenType.RightParenthesis)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
