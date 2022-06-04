using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Unary expressions")]
    [Theory]
    [InlineData(TokenType.OperatorPlus, Operator.NumberPromotion)]
    [InlineData(TokenType.OperatorMinus, Operator.ArithmeticNegation)]
    [InlineData(TokenType.OperatorBang, Operator.LogicalNegation)]
    public void UnaryExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        var opToken = Helpers.GetDefaultToken(operatorTokenType);

        const string identifierName = "a";
        var token = new Token(TokenType.Identifier, identifierName);
        var expectedExpression = new IdentifierExpression(identifierName);

        var lexerMock = new LexerMock(opToken, token, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var expression = expressionStatement.Expression.As<UnaryExpression>();
        expression.Should().NotBeNull();
        expression!.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);
        expression.Operator.Should().Be(expectedOperator);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [InlineData(TokenType.OperatorLess, Operator.PatternMatchingLessThanComparison)]
    [InlineData(TokenType.OperatorLessEquals, Operator.PatternMatchingLessOrEqualComparison)]
    [InlineData(TokenType.OperatorGreater, Operator.PatternMatchingGreaterThanComparison)]
    [InlineData(TokenType.OperatorGreaterEquals, Operator.PatternMatchingGreaterOrEqualComparison)]
    [InlineData(TokenType.OperatorEqualsEquals, Operator.PatternMatchingEqualComparison)]
    [InlineData(TokenType.OperatorBangEquals, Operator.PatternMatchingNotEqualComparison)]
    public void UnaryPatternMatchingExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordMatch),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftBrace),
            Helpers.GetDefaultToken(operatorTokenType),
            new Token(TokenType.LiteralInteger, 5L),
            Helpers.GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            Helpers.GetDefaultToken(TokenType.Semicolon),
            Helpers.GetDefaultToken(TokenType.RightBrace)
        };

        var expectedExpression = new LiteralExpression(DataType.Integer, 5L);

        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Branches.Should().HaveCount(1);

        var unaryExpression = patternMatchingExpression.Branches[0].Pattern.As<UnaryExpression>();
        unaryExpression.Should().NotBeNull();
        unaryExpression.Operator.Should().Be(expectedOperator);
        unaryExpression.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [InlineData(new[] { TokenType.KeywordIs }, TokenType.KeywordInt, Operator.PatternMatchingEqualTypeCheck, DataType.Integer)]
    [InlineData(new[] { TokenType.KeywordIs, TokenType.KeywordNot }, TokenType.KeywordNull, Operator.PatternMatchingNotEqualTypeCheck, DataType.Null)]
    public void TypeCheckingUnaryPatternMatchingExpressionsShouldBeParsedCorrectly(TokenType[] operatorTokenTypes, TokenType typeTokenType, Operator expectedOperator, DataType expectedType)
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordMatch),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftBrace)
        }.Concat(operatorTokenTypes.Select(Helpers.GetDefaultToken)).Concat(new[]
        {
            Helpers.GetDefaultToken(typeTokenType),
            Helpers.GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            Helpers.GetDefaultToken(TokenType.Semicolon),
            Helpers.GetDefaultToken(TokenType.RightBrace)
        }).ToArray();

        var expectedExpression = new TypeExpression(expectedType);

        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Branches.Should().HaveCount(1);

        var unaryExpression = patternMatchingExpression.Branches[0].Pattern.As<UnaryExpression>();
        unaryExpression.Should().NotBeNull();
        unaryExpression.Operator.Should().Be(expectedOperator);
        unaryExpression.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
