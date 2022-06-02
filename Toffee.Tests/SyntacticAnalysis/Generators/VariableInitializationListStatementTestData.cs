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
        var constToken = Helpers.GetDefaultToken(TokenType.KeywordConst);
        var assignmentToken = Helpers.GetDefaultToken(TokenType.OperatorEquals);
        var commaToken = Helpers.GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                initToken,
                new(TokenType.Identifier, "a")
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
                new(TokenType.Identifier, "a")
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a")
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
                new Token(TokenType.LiteralInteger, 5L)
            },
            new VariableInitialization[]
            {
                new("a", new LiteralExpression(DataType.Integer, 5L))
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
                new Token(TokenType.LiteralInteger, 5L)
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a", InitialValue: new LiteralExpression(DataType.Integer, 5L))
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
                new(TokenType.Identifier, "b")
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
                new Token(TokenType.LiteralInteger, 5L),
                commaToken,
                new(TokenType.Identifier, "b")
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a", InitialValue: new LiteralExpression(DataType.Integer, 5L)),
                new("b")
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
