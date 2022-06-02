using System;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTest
{
    [Trait("Category", "Expression statements")]
    [Theory]
    [ClassData(typeof(ExpressionStatementTestData))]
    public void ExpressionStatementsShouldBeParsedCorrectly(Token[] tokenSequence, Type expectedExpressionType)
    {
        var lexerMock = new LexerMock(tokenSequence);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);
        expressionStatement.Expression.Should().BeOfType(expectedExpressionType);
    }
}
