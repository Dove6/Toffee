using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Grouping expressions")]
    [Trait("Category", "Nesting")]
    [Fact]
    public void GroupingExpressionsShouldBeParsedCorrectly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.OperatorPlus),
            new Token(TokenType.LiteralInteger, 5L),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.RightParenthesis)
        };

        var expectedExpression = new GroupingExpression(new GroupingExpression(new BinaryExpression(
            new GroupingExpression(new IdentifierExpression("a")),
            Operator.Addition,
            new LiteralExpression(DataType.Integer, 5L))));

        var lexerMock = new LexerMock(tokenSequence);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);
    }
}
