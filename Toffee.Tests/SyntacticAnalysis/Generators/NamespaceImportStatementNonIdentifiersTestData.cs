using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class NamespaceImportStatementNonIdentifiersTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var pullToken = Helpers.GetDefaultToken(TokenType.KeywordPull);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var dotToken = Helpers.GetDefaultToken(TokenType.OperatorDot);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        // basic
        yield return new object[]
        {
            new[]
            {
                pullToken,
                new(TokenType.LiteralInteger, 1234ul),
                semicolonToken
            },
            new UnexpectedToken(new Position(1, 1, 1), TokenType.LiteralInteger, TokenType.Identifier)
        };
        // after dot
        yield return new object[]
        {
            new[]
            {
                pullToken,
                new(TokenType.Identifier, "a"),
                dotToken,
                new(TokenType.LiteralString, "b"),
                semicolonToken
            },
            new UnexpectedToken(new Position(3, 1, 3), TokenType.LiteralString, TokenType.Identifier)
        };
        // before dot
        yield return new object[]
        {
            new[]
            {
                pullToken,
                new(TokenType.LiteralFloat, 3.14),
                dotToken,
                new(TokenType.Identifier, "a"),
                semicolonToken
            },
            new UnexpectedToken(new Position(1, 1, 1), TokenType.LiteralFloat, TokenType.Identifier)
        };
        // in the middle
        yield return new object[]
        {
            new[]
            {
                pullToken,
                new(TokenType.Identifier, "a"),
                dotToken,
                new(TokenType.Identifier, "b"),
                leftParenthesisToken,
                rightParenthesisToken,
                dotToken,
                new(TokenType.Identifier, "c"),
                semicolonToken
            },
            new UnexpectedToken(new Position(4, 1, 4), TokenType.LeftParenthesis, TokenType.Identifier)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
