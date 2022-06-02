using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class FunctionDefinitionExpressionTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var functiToken = Helpers.GetDefaultToken(TokenType.KeywordFuncti);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var leftBrace = Helpers.GetDefaultToken(TokenType.LeftBrace);
        var rightBrace = Helpers.GetDefaultToken(TokenType.RightBrace);
        var constToken = Helpers.GetDefaultToken(TokenType.KeywordConst);
        var bangToken = Helpers.GetDefaultToken(TokenType.OperatorBang);
        var commaToken = Helpers.GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "a"),
                rightBrace
            },
            Array.Empty<FunctionParameter>(),
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("a")))
        };
        // with one parameter
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter("a")
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))
        };
        // with one const parameter
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                constToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter(IsConst: true, Name: "a")
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))
        };
        // with one required parameter
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                bangToken,
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter("a", IsNullAllowed: false)
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))
        };
        // with more than one parameter
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                commaToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "c"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter("a"),
                new FunctionParameter("b")
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("c")))
        };
        // with more than one parameter (including one const and non-nullable)
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                constToken,
                new(TokenType.Identifier, "a"),
                bangToken,
                commaToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "c"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter(IsConst: true, Name: "a", IsNullAllowed: false),
                new FunctionParameter("b")
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("c")))
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
