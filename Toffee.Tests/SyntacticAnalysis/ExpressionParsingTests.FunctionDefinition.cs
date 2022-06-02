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
        var lexerMock = new LexerMock(tokenSequence);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var functionDefinitionExpression = expressionStatement.Expression.As<FunctionDefinitionExpression>();
        functionDefinitionExpression.Should().NotBeNull();
        functionDefinitionExpression.Parameters.ToArray().Should().BeEquivalentTo(expectedParameters, Helpers.ProvideOptions);
        functionDefinitionExpression.Body.Should().BeEquivalentTo(expectedBody, Helpers.ProvideOptions);
    }
}
