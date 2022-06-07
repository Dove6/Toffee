using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Type cast expressions")]
    [Theory]
    [InlineData(TokenType.KeywordInt, DataType.Integer)]
    [InlineData(TokenType.KeywordFloat, DataType.Float)]
    [InlineData(TokenType.KeywordString, DataType.String)]
    [InlineData(TokenType.KeywordBool, DataType.Bool)]
    public void TypeCastExpressionsShouldBeParsedCorrectly(TokenType literalTokenType, DataType expectedCastType)
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(literalTokenType),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var expression = expressionStatement.Expression.As<TypeCastExpression>();
        expression.Should().NotBeNull();
        expression.Type.Should().Be(expectedCastType);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
