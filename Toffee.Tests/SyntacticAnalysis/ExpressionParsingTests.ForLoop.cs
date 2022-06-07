using System;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "For loop expressions")]
    [Theory]
    [ClassData(typeof(ForLoopExpressionTestData))]
    public void ForLoopExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, string? expectedCounterName, ForLoopRange expectedRange, Expression expectedBody, params Type[] expectedWarnings)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeFalse();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var forLoopExpression = expressionStatement.Expression.As<ForLoopExpression>();
        forLoopExpression.Should().NotBeNull();
        forLoopExpression.CounterName.Should().Be(expectedCounterName);
        forLoopExpression.Range.Should().BeEquivalentTo(expectedRange, Helpers.ProvideOptions);
        forLoopExpression.Body.Should().BeEquivalentTo(expectedBody, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);

        for (var i = 0; i < expectedWarnings.Length; i++)
            errorHandlerMock.HandledWarnings[i].Should().BeOfType(expectedWarnings[i]);
    }

    [Trait("Category", "For loop expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(ForLoopSpecificationMissingParenthesesTestData))]
    public void MissingParenthesesInForLoopSpecificationsShouldBeDetectedProperly(Token[] tokenSequence, Expression expectedExpression, params ParserError[] expectedErrors)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
        hadError.Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var forLoopExpression = expressionStatement.Expression.As<ForLoopExpression>();
        forLoopExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        for (var i = 0; i < expectedErrors.Length; i++)
            errorHandlerMock.HandledErrors[i].Should().BeEquivalentTo(expectedErrors[i]);

        errorHandlerMock.HandledWarnings.Count.Should().Be(1);
        errorHandlerMock.HandledWarnings[0].Should().BeOfType<IgnoredResultExpression>();
    }

    [Trait("Category", "For loop expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(ForLoopSpecificationMissingPartsTestData))]
    public void MissingPartsOfForLoopSpecificationsShouldBeDetectedProperly(Token[] tokenSequence, ParserError expectedError)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeTrue();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "For loop expressions")]
    [Trait("Category", "Negative")]
    [Fact]
    public void MissingBodyOfForLoopExpressionsShouldBeDetectedProperly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordFor),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new ExpectedExpression(new Position(4, 1, 4), TokenType.Semicolon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeTrue();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
