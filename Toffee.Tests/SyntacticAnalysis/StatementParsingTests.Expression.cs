using System;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTests
{
    [Trait("Category", "Expression statements")]
    [Theory]
    [ClassData(typeof(ExpressionStatementTestData))]
    public void ExpressionStatementsShouldBeParsedCorrectly(Token[] tokenSequence, Type expectedExpressionType, params Type[] expectedWarnings)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);
        expressionStatement.Expression.Should().BeOfType(expectedExpressionType);

        Assert.False(errorHandlerMock.HadErrors);

        for (var i = 0; i < expectedWarnings.Length; i++)
            errorHandlerMock.HandledWarnings[i].Should().BeOfType(expectedWarnings[i]);
    }
}
