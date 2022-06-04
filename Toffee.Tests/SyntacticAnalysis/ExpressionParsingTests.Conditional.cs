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
    public void ConditionalExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, ConditionalElement expectedIfPart, ConditionalElement[] expectedElifParts, Expression? expectedElsePart)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var conditionalExpression = expressionStatement.Expression.As<ConditionalExpression>();
        conditionalExpression.Should().NotBeNull();
        conditionalExpression.IfPart.Should().BeEquivalentTo(expectedIfPart, Helpers.ProvideOptions);
        conditionalExpression.ElifParts.ToArray().Should().BeEquivalentTo(expectedElifParts, Helpers.ProvideOptions);
        conditionalExpression.ElsePart.Should().BeEquivalentTo(expectedElsePart, Helpers.ProvideOptions);

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

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
