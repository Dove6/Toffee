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
        var lexerMock = new LexerMock(tokenSequence);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var blockExpression = expressionStatement.Expression.As<BlockExpression>();
        blockExpression.Should().NotBeNull();
        blockExpression!.Statements.ToArray().Should().BeEquivalentTo(expectedStatementList, Helpers.ProvideOptions);
        blockExpression.UnterminatedStatement.Should().BeEquivalentTo(expectedUnterminatedStatement, Helpers.ProvideOptions);
    }
}
