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
    public void BlockExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Statement[] expectedStatementList, Statement? expectedUnterminatedStatement)
    {
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var blockExpression = expressionStatement.Expression.As<BlockExpression>();
        blockExpression.Should().NotBeNull();
        blockExpression!.Statements.ToArray().Should().BeEquivalentTo(expectedStatementList, Helpers.ProvideOptions);
        blockExpression.ResultStatement.Should().BeEquivalentTo(expectedUnterminatedStatement, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
