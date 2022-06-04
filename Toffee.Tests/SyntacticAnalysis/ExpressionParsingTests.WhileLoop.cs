using FluentAssertions;
using Toffee.LexicalAnalysis;
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
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
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
}
