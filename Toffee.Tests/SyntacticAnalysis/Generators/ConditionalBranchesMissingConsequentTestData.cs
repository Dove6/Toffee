using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class ConditionalBranchesMissingConsequentTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var ifToken = Helpers.GetDefaultToken(TokenType.KeywordIf);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var elifToken = Helpers.GetDefaultToken(TokenType.KeywordElif);
        var elseToken = Helpers.GetDefaultToken(TokenType.KeywordElse);
        // if branch
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                semicolonToken
            },
            new ExpectedExpression(new Position(4, 1, 4), TokenType.Semicolon)
        };
        // else branch
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
                semicolonToken
            },
            new ExpectedExpression(new Position(6, 1, 6), TokenType.Semicolon)
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
                semicolonToken
            },
            new ExpectedExpression(new Position(9, 1, 9), TokenType.Semicolon)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
