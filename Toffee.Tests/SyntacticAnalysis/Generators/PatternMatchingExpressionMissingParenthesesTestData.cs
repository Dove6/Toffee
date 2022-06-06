using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class PatternMatchingExpressionMissingParenthesesTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var matchToken = Helpers.GetDefaultToken(TokenType.KeywordMatch);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var leftBrace = Helpers.GetDefaultToken(TokenType.LeftBrace);
        var rightBrace = Helpers.GetDefaultToken(TokenType.RightBrace);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        // missing left parenthesis
        yield return new object[]
        {
            new[]
            {
                matchToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                rightBrace,
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>()),
            new ParserError[]
            {
                new UnexpectedToken(new Position(1, 1, 1), TokenType.Identifier, TokenType.LeftParenthesis)
            },
            typeof(DefaultBranchMissing)
        };
        // missing right parenthesis
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                leftBrace,
                rightBrace,
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>()),
            new ParserError[]
            {
                new UnexpectedToken(new Position(3, 1, 3), TokenType.LeftBrace, TokenType.RightParenthesis)
            },
            typeof(DefaultBranchMissing)
        };
        // missing both parentheses
        yield return new object[]
        {
            new[]
            {
                matchToken,
                new(TokenType.Identifier, "a"),
                leftBrace,
                rightBrace,
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>()),
            new ParserError[]
            {
                new UnexpectedToken(new Position(1, 1, 1), TokenType.Identifier, TokenType.LeftParenthesis),
                new UnexpectedToken(new Position(2, 1, 2), TokenType.LeftBrace, TokenType.RightParenthesis)
            },
            typeof(DefaultBranchMissing)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
