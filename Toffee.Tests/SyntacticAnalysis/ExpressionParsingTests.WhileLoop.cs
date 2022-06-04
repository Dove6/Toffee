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
    public void WhileLoopExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedCondition, Expression expectedBody)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var whileLoopExpression = expressionStatement.Expression.As<WhileLoopExpression>();
        whileLoopExpression.Should().NotBeNull();
        whileLoopExpression.Condition.Should().BeEquivalentTo(expectedCondition, Helpers.ProvideOptions);
        whileLoopExpression.Body.Should().BeEquivalentTo(expectedBody, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
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

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
