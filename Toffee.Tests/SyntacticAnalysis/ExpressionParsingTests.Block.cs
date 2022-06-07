using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Block expressions")]
    [Theory]
    [ClassData(typeof(BlockExpressionTestData))]
    public void BlockExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Statement[] expectedStatementList, Expression? expectedResultExpression)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var blockExpression = expressionStatement.Expression.As<BlockExpression>();
        blockExpression.Should().NotBeNull();
        blockExpression!.Statements.ToArray().Should().BeEquivalentTo(expectedStatementList, Helpers.ProvideOptions);
        blockExpression.ResultExpression.Should().BeEquivalentTo(expectedResultExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Block expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(BlockExpressionMissingClosingBraceTestData))]
    public void MissingClosingBraceInBlockExpressionsShouldBeDetectedProperly(Token[] tokenSequence, Statement[] expectedStatementList, Expression? expectedResultExpression, ParserError expectedError, bool shouldStatementBeTerminated)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(shouldStatementBeTerminated);

        var blockExpression = expressionStatement.Expression.As<BlockExpression>();
        blockExpression.Should().NotBeNull();
        blockExpression!.Statements.ToArray().Should().BeEquivalentTo(expectedStatementList, Helpers.ProvideOptions);
        blockExpression.ResultExpression.Should().BeEquivalentTo(expectedResultExpression, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Block expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(BlockExpressionMissingSemicolonTestData))]
    public void MissingSemicolonInBlockExpressionsShouldBeDetectedProperly(Token[] tokenSequence, Expression expectedExpression, params ParserError[] expectedErrors)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var blockExpression = expressionStatement.Expression.As<BlockExpression>();
        blockExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        for (var i = 0; i < expectedErrors.Length; i++)
            errorHandlerMock.HandledErrors[i].Should().BeEquivalentTo(expectedErrors[i]);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
