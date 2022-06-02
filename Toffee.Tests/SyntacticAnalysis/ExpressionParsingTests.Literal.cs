using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Literal expressions")]
    [Theory]
    [InlineData(TokenType.LiteralInteger, 1234L, DataType.Integer, 1234L)]
    [InlineData(TokenType.LiteralFloat, 3.14, DataType.Float, 3.14)]
    [InlineData(TokenType.LiteralString, "abcd", DataType.String, "abcd")]
    [InlineData(TokenType.KeywordTrue, "true", DataType.Bool, true)]
    [InlineData(TokenType.KeywordFalse, "false", DataType.Bool, false)]
    [InlineData(TokenType.KeywordNull, null, DataType.Null, null)]
    public void LiteralExpressionsShouldBeParsedCorrectly(TokenType literalTokenType, object? literalTokenContent, DataType literalType, object? literalValue)
    {
        var literalToken = new Token(literalTokenType, literalTokenContent);

        var lexerMock = new LexerMock(literalToken);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var expression = expressionStatement.Expression.As<LiteralExpression>();
        expression.Should().NotBeNull();
        expression!.Type.Should().Be(literalType);
        expression.Value.Should().Be(literalValue);
    }
}
