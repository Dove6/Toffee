using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTest
{
    [Trait("Category", "Break statements")]
    [Fact]
    public void BreakStatementsShouldBeParsedCorrectly()
    {
        var breakToken = Helpers.GetDefaultToken(TokenType.KeywordBreak);

        var lexerMock = new LexerMock(breakToken);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var breakStatement = parser.CurrentStatement.As<BreakStatement>();
        breakStatement.Should().NotBeNull();
        breakStatement!.IsTerminated.Should().Be(false);
    }
}
