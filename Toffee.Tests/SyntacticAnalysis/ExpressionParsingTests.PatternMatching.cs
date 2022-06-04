using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [ClassData(typeof(PatternMatchingExpressionTestData))]
    public void PatternMatchingExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedArgument, PatternMatchingBranch[] expectedBranches, Expression? expectedDefault)
    {
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Argument.Should().BeEquivalentTo(expectedArgument, Helpers.ProvideOptions);
        patternMatchingExpression.Branches.ToArray().Should().BeEquivalentTo(expectedBranches, Helpers.ProvideOptions);
        patternMatchingExpression.Default.Should().BeEquivalentTo(expectedDefault, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
