using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "For loop expressions")]
    [Theory]
    [ClassData(typeof(ForLoopExpressionTestData))]
    public void ForLoopExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, string? expectedCounterName, ForLoopRange expectedRange, Expression expectedBody)
    {
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var forLoopExpression = expressionStatement.Expression.As<ForLoopExpression>();
        forLoopExpression.Should().NotBeNull();
        forLoopExpression.CounterName.Should().Be(expectedCounterName);
        forLoopExpression.Range.Should().BeEquivalentTo(expectedRange, Helpers.ProvideOptions);
        forLoopExpression.Body.Should().BeEquivalentTo(expectedBody, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
