using System.Collections.Generic;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Namespace access expressions")]
    [Fact]
    public void NamespaceAccessExpressionsShouldBeHandledCorrectly()
    {
        var tokenSequence = new[]
        {
            new(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.OperatorDot),
            new(TokenType.Identifier, "b"),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedExpression = new NamespaceAccessExpression(new List<string> { "a" }, "b");

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.IsTerminated.Should().BeTrue();

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Namespace access expressions")]
    [Fact]
    public void SingleIdentifierShouldNotBeParsedAsNamespaceAccessExpression()
    {
        var tokenSequence = new[]
        {
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedExpression = new IdentifierExpression("a");

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.IsTerminated.Should().BeTrue();

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Namespace access expressions")]
    [Trait("Category", "Negative")]
    [Fact]
    public void NonIdentifiersInNamespaceAccessExpressionsShouldBeDetectedProperly()
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

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
