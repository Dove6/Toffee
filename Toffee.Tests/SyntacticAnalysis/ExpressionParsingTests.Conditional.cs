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
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var conditionalExpression = expressionStatement.Expression.As<ConditionalExpression>();
        conditionalExpression.Should().NotBeNull();
        conditionalExpression.IfPart.Should().BeEquivalentTo(expectedIfPart, Helpers.ProvideOptions);
        conditionalExpression.ElifParts.ToArray().Should().BeEquivalentTo(expectedElifParts, Helpers.ProvideOptions);
        conditionalExpression.ElsePart.Should().BeEquivalentTo(expectedElsePart, Helpers.ProvideOptions);
    }
}
