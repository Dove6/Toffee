using System.Collections.Generic;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Identifier expressions")]
    [Fact]
    public void IdentifierExpressionsWithNoNamespaceShouldBeParsedCorrectly()
    {
        const string identifierName = "ident";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var lexerMock = new LexerMock(identifierToken, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeFalse();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var expression = expressionStatement.Expression.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression!.Name.Should().Be(identifierName);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Namespace access expressions")]
    [Fact]
    public void IdentifierExpressionsWithNamespaceShouldBeHandledCorrectly()
    {
        var tokenSequence = new[]
        {
            new(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.OperatorDot),
            new(TokenType.Identifier, "b"),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedExpression = new IdentifierExpression(new List<string> { "a" }, "b");

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeFalse();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.IsTerminated.Should().BeTrue();

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Namespace access expressions")]
    [Trait("Category", "Negative")]
    [Fact]
    public void NonIdentifiersInNamespaceInIdentifierExpressionsShouldBeDetectedProperly()
    {
        var tokenSequence = new[]
        {
            new(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.OperatorDot),
            new(TokenType.LiteralString, "b"),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new UnexpectedToken(new Position(2, 1, 2), TokenType.LiteralString, TokenType.Identifier);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeTrue();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
