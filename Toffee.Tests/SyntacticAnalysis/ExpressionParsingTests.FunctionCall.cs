using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Function call expressions")]
    [Theory]
    [ClassData(typeof(FunctionCallExpressionTestData))]
    public void FunctionCallExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedCalledExpression, Expression[] expectedArguments)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeFalse();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var functionCallExpression = expressionStatement.Expression.As<FunctionCallExpression>();
        functionCallExpression.Should().NotBeNull();
        functionCallExpression.Callee.Should().BeEquivalentTo(expectedCalledExpression, Helpers.ProvideOptions);
        functionCallExpression.Arguments.ToArray().Should().BeEquivalentTo(expectedArguments, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Function call expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(FunctionCallExpressionMissingClosingParenthesisTestData))]
    public void MissingClosingParenthesisInFunctionCallExpressionsShouldBeDetectedProperly(Token[] tokenSequence, Expression expectedExpression, ParserError expectedError)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
        hadError.Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var functionCallExpression = expressionStatement.Expression.As<FunctionCallExpression>();
        functionCallExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Function call expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingArgumentInFunctionCallExpressionsShouldBeDetectedProperly(bool missingAfterComma)
    {
        var commaAndArgument = new Token[2];
        commaAndArgument[missingAfterComma ? 1 : 0] = Helpers.GetDefaultToken(TokenType.Comma);
        commaAndArgument[missingAfterComma ? 0 : 1] = new Token(TokenType.Identifier, "b");
        var tokenSequence = new[]
        {
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
        }.Concat(commaAndArgument)
            .Append(Helpers.GetDefaultToken(TokenType.RightParenthesis))
            .AppendSemicolon();

        var errorPosition = 2u + (missingAfterComma ? 2u : 0u);
        var expectedError = new ExpectedExpression(new Position(errorPosition, 1, errorPosition),
            missingAfterComma ? TokenType.RightParenthesis : TokenType.Comma);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeTrue();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
