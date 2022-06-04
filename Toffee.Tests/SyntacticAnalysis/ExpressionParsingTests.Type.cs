using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Type expressions")]
    [Theory]
    [InlineData(TokenType.KeywordInt, DataType.Integer)]
    [InlineData(TokenType.KeywordFloat, DataType.Float)]
    [InlineData(TokenType.KeywordString, DataType.String)]
    [InlineData(TokenType.KeywordBool, DataType.Bool)]
    [InlineData(TokenType.KeywordFunction, DataType.Function)]
    [InlineData(TokenType.KeywordNull, DataType.Null)]
    public void TypeExpressionsShouldBeParsedCorrectly(TokenType typeTokenType, DataType type)
    {
        var tokenSequence = new[]
        {
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.KeywordIs),
            Helpers.GetDefaultToken(typeTokenType)
        };

        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var binaryExpression = expressionStatement.Expression.As<BinaryExpression>();
        binaryExpression.Should().NotBeNull();

        var typeExpression = binaryExpression.Right.As<TypeExpression>();
        typeExpression.Should().NotBeNull();
        typeExpression!.Type.Should().Be(type);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
