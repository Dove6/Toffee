using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class VariableInitializationListStatementMissingVariableTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var initToken = Helpers.GetDefaultToken(TokenType.KeywordInit);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var constToken = Helpers.GetDefaultToken(TokenType.KeywordConst);
        var assignmentToken = Helpers.GetDefaultToken(TokenType.OperatorEquals);
        var commaToken = Helpers.GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                initToken,
                semicolonToken
            },
            new UnexpectedToken(new Position(1, 1, 1), TokenType.Semicolon, TokenType.Identifier)
        };
        // after basic
        yield return new object[]
        {
            new[]
            {
                initToken,
                new(TokenType.Identifier, "a"),
                commaToken,
                semicolonToken
            },
            new UnexpectedToken(new Position(3, 1, 3), TokenType.Semicolon, TokenType.Identifier)
        };
        // after const
        yield return new object[]
        {
            new[]
            {
                initToken,
                constToken,
                new(TokenType.Identifier, "a"),
                commaToken,
                semicolonToken
            },
            new ImplicitConstInitialization(new Position(1, 1, 1), "a"),
            new UnexpectedToken(new Position(4, 1, 4), TokenType.Semicolon, TokenType.Identifier)
        };
        // after initialization
        yield return new object[]
        {
            new[]
            {
                initToken,
                new(TokenType.Identifier, "a"),
                assignmentToken,
                new Token(TokenType.LiteralInteger, 5ul),
                commaToken,
                semicolonToken
            },
            new UnexpectedToken(new Position(5, 1, 5), TokenType.Semicolon, TokenType.Identifier)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
