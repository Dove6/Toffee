using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class ParenthesizedExpressionMissingParenthesesTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var ifToken = Helpers.GetDefaultToken(TokenType.KeywordIf);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        // left parenthesis
        yield return new object[]
        {
            new[]
            {
                ifToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
            {
                new(new IdentifierExpression("a"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("b")))
            }),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.Identifier, TokenType.LeftParenthesis)
        };
        // right parenthesis
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
            {
                new(new IdentifierExpression("a"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("b")))
            }),
            new UnexpectedToken(new Position(3, 1, 3), TokenType.Identifier, TokenType.RightParenthesis)
        };
        // both parentheses
        yield return new object[]
        {
            new[]
            {
                ifToken,
                new(TokenType.Identifier, "a"),
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
            {
                new(new IdentifierExpression("a"),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("b")))
            }),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.Identifier, TokenType.LeftParenthesis),
            new UnexpectedToken(new Position(2, 1, 2), TokenType.Identifier, TokenType.RightParenthesis)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
