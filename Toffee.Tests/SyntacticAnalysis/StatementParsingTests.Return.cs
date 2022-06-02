using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTest
{
    [Trait("Category", "Return statements")]
    [Fact]
    public void EmptyReturnStatementsShouldBeParsedCorrectly()
    {
        var returnToken = Helpers.GetDefaultToken(TokenType.KeywordReturn);

        var lexerMock = new LexerMock(returnToken);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var returnStatement = parser.CurrentStatement.As<ReturnStatement>();
        returnStatement.Should().NotBeNull();
        returnStatement!.IsTerminated.Should().Be(false);
        returnStatement.Value.Should().BeNull();
    }

    [Trait("Category", "Return statements")]
    [Fact]
    public void ReturnStatementsContainingExpressionsShouldBeParsedCorrectly()
    {
        var returnToken = Helpers.GetDefaultToken(TokenType.KeywordReturn);

        const string identifierName = "a";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var lexerMock = new LexerMock(returnToken, identifierToken);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var returnStatement = parser.CurrentStatement.As<ReturnStatement>();
        returnStatement.Should().NotBeNull();
        returnStatement!.IsTerminated.Should().Be(false);
        returnStatement.Value.Should().NotBeNull();

        var expression = returnStatement.Value.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression.Name.Should().Be(identifierName);
    }
}
