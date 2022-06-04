using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class NamespaceAccessExpressionNonIdentifiersTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var dotToken = Helpers.GetDefaultToken(TokenType.OperatorDot);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        // after dot
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                dotToken,
                new(TokenType.LiteralString, "b"),
                semicolonToken
            },
            new BinaryExpression(new IdentifierExpression("a"), Operator.NamespaceAccess,
                new LiteralExpression(DataType.String, "b")),
            new ExpectedIdentifier(new Position(2, 1, 2), typeof(LiteralExpression))
        };
        // before dot
        yield return new object[]
        {
            new[]
            {
                new(TokenType.LiteralFloat, 3.14),
                dotToken,
                new(TokenType.Identifier, "a"),
                semicolonToken
            },
            new BinaryExpression(new LiteralExpression(DataType.Float, 3.14), Operator.NamespaceAccess,
                new IdentifierExpression("a")),
            new ExpectedIdentifier(new Position(0, 1, 0), typeof(LiteralExpression))
        };
        // in the middle
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                dotToken,
                new(TokenType.Identifier, "b"),
                leftParenthesisToken,
                rightParenthesisToken,
                dotToken,
                new(TokenType.Identifier, "c"),
                semicolonToken
            },
            new BinaryExpression(new FunctionCallExpression(new BinaryExpression(new IdentifierExpression("a"),
                    Operator.NamespaceAccess,
                    new IdentifierExpression("b")), new List<Expression>()),
                Operator.NamespaceAccess,
                new IdentifierExpression("c")),
            new ExpectedIdentifier(new Position(0, 1, 0), typeof(FunctionCallExpression))
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
