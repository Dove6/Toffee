using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "While loop expressions")]
    [Theory]
    [ClassData(typeof(WhileLoopExpressionTestData))]
    public void WhileLoopExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedExpression)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);

        errorHandlerMock.HandledWarnings.Count.Should().Be(1);
        errorHandlerMock.HandledWarnings[0].Should().BeOfType<IgnoredResultExpression>();
    }

    [Trait("Category", "While loop expressions")]
    [Trait("Category", "Negative")]
    [Fact]
    public void MissingBodyOfWhileLoopExpressionsShouldBeDetectedProperly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordWhile),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new ExpectedExpression(new Position(4, 1, 4), TokenType.Semicolon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeFalse();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
