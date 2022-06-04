using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class ForLoopSpecificationMissingParenthesesTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var forToken = Helpers.GetDefaultToken(TokenType.KeywordFor);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        // left parenthesis
        yield return new object[]
        {
            new[]
            {
                forToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new ForLoopExpression(new ForLoopRange(new IdentifierExpression("a")), new IdentifierExpression("b")),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.Identifier, TokenType.LeftParenthesis)
        };
        // right parenthesis
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new ForLoopExpression(new ForLoopRange(new IdentifierExpression("a")), new IdentifierExpression("b")),
            new UnexpectedToken(new Position(3, 1, 3), TokenType.Identifier, TokenType.LeftParenthesis)
        };
        // both parentheses
        yield return new object[]
        {
            new[]
            {
                forToken,
                new(TokenType.Identifier, "a"),
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new ForLoopExpression(new ForLoopRange(new IdentifierExpression("a")), new IdentifierExpression("b")),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.Identifier, TokenType.LeftParenthesis),
            new UnexpectedToken(new Position(2, 1, 2), TokenType.Identifier, TokenType.LeftParenthesis)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
