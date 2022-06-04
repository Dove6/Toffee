using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Literal expressions")]
    [Theory]
    [InlineData(TokenType.LiteralInteger, 1234ul, DataType.Integer, 1234ul)]
    [InlineData(TokenType.LiteralFloat, 3.14, DataType.Float, 3.14)]
    [InlineData(TokenType.LiteralString, "abcd", DataType.String, "abcd")]
    [InlineData(TokenType.KeywordTrue, "true", DataType.Bool, true)]
    [InlineData(TokenType.KeywordFalse, "false", DataType.Bool, false)]
    [InlineData(TokenType.KeywordNull, null, DataType.Null, null)]
    public void LiteralExpressionsShouldBeParsedCorrectly(TokenType literalTokenType, object? literalTokenContent, DataType literalType, object? literalValue)
    {
        var literalToken = new Token(literalTokenType, literalTokenContent);

        var lexerMock = new LexerMock(literalToken, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var expression = expressionStatement.Expression.As<LiteralExpression>();
        expression.Should().NotBeNull();
        expression!.Type.Should().Be(literalType);
        expression.Value.Should().Be(literalValue);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
