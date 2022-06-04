using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class FunctionDefinitionExpressionMissingParenthesisTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var functiToken = Helpers.GetDefaultToken(TokenType.KeywordFuncti);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var leftBrace = Helpers.GetDefaultToken(TokenType.LeftBrace);
        var rightBrace = Helpers.GetDefaultToken(TokenType.RightBrace);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        // basic, missing left parenthesis
        yield return new object[]
        {
            new[]
            {
                functiToken,
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "a"),
                rightBrace,
                semicolonToken
            },
            new FunctionDefinitionExpression(new List<FunctionParameter>(),
                new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("a")))),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.RightParenthesis, TokenType.LeftParenthesis)
        };
        // basic, missing right parenthesis
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "a"),
                rightBrace,
                semicolonToken
            },
            new FunctionDefinitionExpression(new List<FunctionParameter>(),
                new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("a")))),
            new UnexpectedToken(new Position(2, 1, 2), TokenType.LeftBrace, TokenType.RightParenthesis)
        };
        // basic, missing both parentheses
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftBrace,
                new(TokenType.Identifier, "a"),
                rightBrace,
                semicolonToken
            },
            new FunctionDefinitionExpression(new List<FunctionParameter>(),
                new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("a")))),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.LeftBrace, TokenType.LeftParenthesis),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.LeftBrace, TokenType.RightParenthesis)
        };
        // with one parameter, missing left parenthesis
        yield return new object[]
        {
            new[]
            {
                functiToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace,
                semicolonToken
            },
            new FunctionDefinitionExpression(new List<FunctionParameter> { new("a") },
                new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.Identifier, TokenType.LeftParenthesis)
        };
        // with one parameter, missing right parenthesis
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace,
                semicolonToken
            },
            new FunctionDefinitionExpression(new List<FunctionParameter> { new("a") },
                new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))),
            new UnexpectedToken(new Position(3, 1, 3), TokenType.LeftBrace, TokenType.RightParenthesis)
        };
        // with one parameter, missing both parenthesis
        yield return new object[]
        {
            new[]
            {
                functiToken,
                new(TokenType.Identifier, "a"),
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace,
                semicolonToken
            },
            new FunctionDefinitionExpression(new List<FunctionParameter> { new("a") },
                new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))),
            new UnexpectedToken(new Position(1, 1, 1), TokenType.Identifier, TokenType.LeftParenthesis),
            new UnexpectedToken(new Position(2, 1, 2), TokenType.LeftBrace, TokenType.RightParenthesis)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
