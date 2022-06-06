using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
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
    [InlineData(TokenType.OperatorLess, Operator.LessThanComparison)]
    [InlineData(TokenType.OperatorLessEquals, Operator.LessOrEqualComparison)]
    [InlineData(TokenType.OperatorGreater, Operator.GreaterThanComparison)]
    [InlineData(TokenType.OperatorGreaterEquals, Operator.GreaterOrEqualComparison)]
    [InlineData(TokenType.OperatorEqualsEquals, Operator.EqualComparison)]
    [InlineData(TokenType.OperatorBangEquals, Operator.NotEqualComparison)]
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
            new Token(TokenType.LiteralInteger, 5ul),
            Helpers.GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            Helpers.GetDefaultToken(TokenType.Semicolon),
            Helpers.GetDefaultToken(TokenType.RightBrace),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedExpression = new LiteralExpression(DataType.Integer, 5ul);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var patternMatchingExpression = expressionStatement.Expression.As<ConditionalExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Branches.Should().HaveCount(1);

        var binaryExpression = patternMatchingExpression.Branches[0].Condition.As<BinaryExpression>();
        binaryExpression.Should().NotBeNull();
        binaryExpression.Operator.Should().Be(expectedOperator);
        binaryExpression.Right.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        errorHandlerMock.HandledWarnings.Count.Should().Be(1);
        errorHandlerMock.HandledWarnings[0].Should().BeOfType<DefaultBranchMissing>();
    }

    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(TokenType.OperatorPlus)]
    [InlineData(TokenType.OperatorMinus)]
    [InlineData(TokenType.OperatorBang)]
    public void MissingExpressionsInUnaryExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType)
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(operatorTokenType),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new ExpectedExpression(new Position(1, 1, 1), TokenType.Semicolon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(TokenType.OperatorLess)]
    [InlineData(TokenType.OperatorLessEquals)]
    [InlineData(TokenType.OperatorGreater)]
    [InlineData(TokenType.OperatorGreaterEquals)]
    [InlineData(TokenType.OperatorEqualsEquals)]
    [InlineData(TokenType.OperatorBangEquals)]
    public void MissingExpressionsInUnaryPatternMatchingExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType)
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordMatch),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftBrace),
            Helpers.GetDefaultToken(operatorTokenType),
            Helpers.GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            Helpers.GetDefaultToken(TokenType.Semicolon),
            Helpers.GetDefaultToken(TokenType.RightBrace),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new ExpectedExpression(new Position(6, 1, 6), TokenType.Colon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Literal expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(0ul, false, false)]
    [InlineData(0ul, true, false)]
    [InlineData(1234ul, false, false)]
    [InlineData(1234ul, true, false)]
    [InlineData(9223372036854775807ul, false, false)]
    [InlineData(9223372036854775807ul, true, false)]
    [InlineData(9223372036854775808ul, false, true)]
    [InlineData(9223372036854775808ul, true, false)]
    [InlineData(9223372036854775809ul, false, true)]
    [InlineData(9223372036854775809ul, true, true)]
    [InlineData(18446744073709551615ul, false, true)]
    [InlineData(18446744073709551615ul, true, true)]
    public void OutOfRangeIntegralUnaryExpressionsShouldBeDetectedProperly(ulong value, bool hasNegativePrefix, bool shouldEmitError)
    {
        var literalToken = new Token(TokenType.LiteralInteger, value);
        var tokenSequence = (hasNegativePrefix
            ? new[] { Helpers.GetDefaultToken(TokenType.OperatorMinus), literalToken }
            : new[] { literalToken })
            .AppendSemicolon();

        Expression expectedExpression = hasNegativePrefix
            ? new UnaryExpression(Operator.ArithmeticNegation, new LiteralExpression(DataType.Integer, value))
            : new LiteralExpression(DataType.Integer, value);

        var expectedError = new IntegerOutOfRange(new Position(0, 1, 0), value, hasNegativePrefix);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        if (shouldEmitError)
            errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);
        else
            Assert.False(errorHandlerMock.HadErrors);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
