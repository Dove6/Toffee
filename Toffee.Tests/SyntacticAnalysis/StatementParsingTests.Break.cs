using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTests
{
    [Trait("Category", "Break statements")]
    [Fact]
    public void BreakStatementsShouldBeParsedCorrectly()
    {
        var breakToken = Helpers.GetDefaultToken(TokenType.KeywordBreak);

        var lexerMock = new LexerMock(breakToken, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var breakStatement = parser.CurrentStatement.As<BreakStatement>();
        breakStatement.Should().NotBeNull();
        breakStatement!.IsTerminated.Should().Be(true);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Break statements")]
    [Fact]
    public void BreakStatementsWithConditionShouldBeParsedCorrectly()
    {
        var breakIfToken = Helpers.GetDefaultToken(TokenType.KeywordBreakIf);

        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);

        const string identifierName = "a";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);

        var lexerMock = new LexerMock(breakIfToken,
            leftParenthesisToken,
            identifierToken,
            rightParenthesisToken,
            Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var breakStatement = parser.CurrentStatement.As<BreakStatement>();
        breakStatement.Should().NotBeNull();
        breakStatement!.IsTerminated.Should().Be(true);

        var expression = breakStatement.Condition.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression!.Name.Should().Be(identifierName);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
