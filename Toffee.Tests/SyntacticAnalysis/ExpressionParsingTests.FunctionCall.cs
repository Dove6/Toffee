using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
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
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var functionCallExpression = expressionStatement.Expression.As<FunctionCallExpression>();
        functionCallExpression.Should().NotBeNull();
        functionCallExpression.Expression.Should().BeEquivalentTo(expectedCalledExpression, Helpers.ProvideOptions);
        functionCallExpression.Arguments.ToArray().Should().BeEquivalentTo(expectedArguments, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
