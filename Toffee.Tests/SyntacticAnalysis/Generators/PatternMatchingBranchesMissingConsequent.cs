using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class PatternMatchingBranchesMissingConsequent : IEnumerable<object[]>
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
        // with non-default branch
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
                semicolonToken,
                rightBrace,
                semicolonToken
            },
            new ExpectedExpression(new Position(7, 1, 7), TokenType.Semicolon)
        };
        // with default branch
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
                semicolonToken,
                rightBrace,
                semicolonToken
            },
            new ExpectedExpression(new Position(7, 1, 7), TokenType.Semicolon)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
