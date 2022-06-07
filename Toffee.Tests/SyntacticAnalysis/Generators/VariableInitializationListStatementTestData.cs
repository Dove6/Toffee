using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class VariableInitializationListStatementTestData : IEnumerable<object[]>
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
                new(TokenType.Identifier, "a"),
                semicolonToken
            },
            new VariableInitialization[]
            {
                new("a")
            }
        };
        // with const
        yield return new object[]
        {
            new[]
            {
                initToken,
                constToken,
                new(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.OperatorEquals),
                Helpers.GetDefaultToken(TokenType.KeywordNull),
                semicolonToken
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a", InitialValue: new LiteralExpression(DataType.Null, null))
            }
        };
        // with initialization
        yield return new object[]
        {
            new[]
            {
                initToken,
                new(TokenType.Identifier, "a"),
                assignmentToken,
                new Token(TokenType.LiteralInteger, 5ul),
                semicolonToken
            },
            new VariableInitialization[]
            {
                new("a", new LiteralExpression(DataType.Integer, 5ul))
            }
        };
        // with const and initialization
        yield return new object[]
        {
            new[]
            {
                initToken,
                constToken,
                new(TokenType.Identifier, "a"),
                assignmentToken,
                new Token(TokenType.LiteralInteger, 5ul),
                semicolonToken
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a", InitialValue: new LiteralExpression(DataType.Integer, 5ul))
            }
        };
        // more than one
        yield return new object[]
        {
            new[]
            {
                initToken,
                new(TokenType.Identifier, "a"),
                commaToken,
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new VariableInitialization[]
            {
                new("a"),
                new("b")
            }
        };
        // more than one, with const and initialization
        yield return new object[]
        {
            new[]
            {
                initToken,
                constToken,
                new(TokenType.Identifier, "a"),
                assignmentToken,
                new Token(TokenType.LiteralInteger, 5ul),
                commaToken,
                new(TokenType.Identifier, "b"),
                semicolonToken
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a", InitialValue: new LiteralExpression(DataType.Integer, 5ul)),
                new("b")
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
