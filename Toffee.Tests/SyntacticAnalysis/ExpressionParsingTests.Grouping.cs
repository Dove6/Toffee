using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Grouping expressions")]
    [Trait("Category", "Nesting")]
    [Fact]
    public void GroupingExpressionsShouldBeParsedCorrectly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.OperatorPlus),
            new Token(TokenType.LiteralInteger, 5ul),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedExpression = new GroupingExpression(new GroupingExpression(new BinaryExpression(
            new GroupingExpression(new IdentifierExpression("a")),
            Operator.Addition,
            new LiteralExpression(DataType.Integer, 5ul))));

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Grouping expressions")]
    [Trait("Category", "Negative")]
    [Fact]
    public void MissingClosingParenthesisInGroupingExpressionsShouldBeDetectedProperly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedExpression = new GroupingExpression(new IdentifierExpression("a"));

        var expectedError = new UnexpectedToken(new Position(2, 1, 2), TokenType.Semicolon, TokenType.RightParenthesis);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Grouping expressions")]
    [Trait("Category", "Negative")]
    [Fact]
    public void MissingExpressionInGroupingExpressionsShouldBeDetectedProperly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new ExpectedExpression(new Position(1, 1, 1), TokenType.RightParenthesis);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeFalse();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
