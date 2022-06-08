using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Conditional expressions")]
    [Theory]
    [ClassData(typeof(ConditionalExpressionTestData))]
    public void ConditionalExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedExpression)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeFalse();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Conditional expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(ConditionalBranchesMissingConsequentTestData))]
    public void MissingConsequentOfConditionalBranchesShouldBeDetectedProperly(Token[] tokenSequence, ParserError expectedError)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeTrue();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
