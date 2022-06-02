using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTest
{
    [Trait("Category", "Break if statements")]
    [Fact]
    public void BreakIfStatementsShouldBeParsedCorrectly()
    {
        var breakIfToken = Helpers.GetDefaultToken(TokenType.KeywordBreakIf);

        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);

        const string identifierName = "a";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);

        var lexerMock = new LexerMock(breakIfToken, leftParenthesisToken, identifierToken, rightParenthesisToken);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var breakIfStatement = parser.CurrentStatement.As<BreakIfStatement>();
        breakIfStatement.Should().NotBeNull();
        breakIfStatement!.IsTerminated.Should().Be(false);

        var expression = breakIfStatement.Condition.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression!.Name.Should().Be(identifierName);
    }
}
