using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTests
{
    [Trait("Category", "Return statements")]
    [Fact]
    public void EmptyReturnStatementsShouldBeParsedCorrectly()
    {
        var returnToken = Helpers.GetDefaultToken(TokenType.KeywordReturn);

        var lexerMock = new LexerMock(returnToken, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeFalse();

        var returnStatement = statement.As<ReturnStatement>();
        returnStatement.Should().NotBeNull();
        returnStatement!.IsTerminated.Should().Be(true);
        returnStatement.Value.Should().BeNull();

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Return statements")]
    [Fact]
    public void ReturnStatementsContainingExpressionsShouldBeParsedCorrectly()
    {
        var returnToken = Helpers.GetDefaultToken(TokenType.KeywordReturn);

        const string identifierName = "a";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var lexerMock = new LexerMock(returnToken, identifierToken, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeFalse();

        var returnStatement = statement.As<ReturnStatement>();
        returnStatement.Should().NotBeNull();
        returnStatement!.IsTerminated.Should().Be(true);
        returnStatement.Value.Should().NotBeNull();

        var expression = returnStatement.Value.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression.Name.Should().Be(identifierName);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
