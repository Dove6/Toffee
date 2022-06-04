using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Function definition expressions")]
    [Theory]
    [ClassData(typeof(FunctionDefinitionExpressionTestData))]
    public void FunctionDefinitionExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, FunctionParameter[] expectedParameters, BlockExpression expectedBody)
    {
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var functionDefinitionExpression = expressionStatement.Expression.As<FunctionDefinitionExpression>();
        functionDefinitionExpression.Should().NotBeNull();
        functionDefinitionExpression.Parameters.ToArray().Should().BeEquivalentTo(expectedParameters, Helpers.ProvideOptions);
        functionDefinitionExpression.Body.Should().BeEquivalentTo(expectedBody, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
