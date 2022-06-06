using System.Collections.Generic;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Binary expressions")]
    [Theory]
    [InlineData(TokenType.OperatorCaret, Operator.Exponentiation)]
    [InlineData(TokenType.OperatorPlus, Operator.Addition)]
    [InlineData(TokenType.OperatorMinus, Operator.Subtraction)]
    [InlineData(TokenType.OperatorAsterisk, Operator.Multiplication)]
    [InlineData(TokenType.OperatorSlash, Operator.Division)]
    [InlineData(TokenType.OperatorPercent, Operator.Remainder)]
    [InlineData(TokenType.OperatorDotDot, Operator.Concatenation)]
    [InlineData(TokenType.OperatorAndAnd, Operator.Conjunction)]
    [InlineData(TokenType.OperatorOrOr, Operator.Disjunction)]
    [InlineData(TokenType.OperatorQueryQuery, Operator.NullCoalescing)]
    [InlineData(TokenType.OperatorQueryGreater, Operator.NullSafePipe)]
    [InlineData(TokenType.OperatorEquals, Operator.Assignment)]
    [InlineData(TokenType.OperatorPlusEquals, Operator.AdditionAssignment)]
    [InlineData(TokenType.OperatorMinusEquals, Operator.SubtractionAssignment)]
    [InlineData(TokenType.OperatorAsteriskEquals, Operator.MultiplicationAssignment)]
    [InlineData(TokenType.OperatorSlashEquals, Operator.DivisionAssignment)]
    [InlineData(TokenType.OperatorPercentEquals, Operator.RemainderAssignment)]
    public void BinaryExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        const string leftIdentifierName = "a";
        var leftToken = new Token(TokenType.Identifier, leftIdentifierName);
        var expectedLeftExpression = new IdentifierExpression(leftIdentifierName);

        var opToken = Helpers.GetDefaultToken(operatorTokenType);

        const string rightIdentifierName = "b";
        var rightToken = new Token(TokenType.Identifier, rightIdentifierName);
        var expectedRightExpression = new IdentifierExpression(rightIdentifierName);

        var lexerMock = new LexerMock(leftToken, opToken, rightToken, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var expression = expressionStatement.Expression.As<BinaryExpression>();
        expression.Should().NotBeNull();
        expression!.Left.Should().BeEquivalentTo(expectedLeftExpression, Helpers.ProvideOptions);
        expression.Operator.Should().Be(expectedOperator);
        expression.Right.Should().BeEquivalentTo(expectedRightExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Binary expressions")]
    [Trait("Category", "Comparison expressions")]
    [Theory]
    [InlineData(TokenType.OperatorLess, Operator.LessThanComparison)]
    [InlineData(TokenType.OperatorLessEquals, Operator.LessOrEqualComparison)]
    [InlineData(TokenType.OperatorGreater, Operator.GreaterThanComparison)]
    [InlineData(TokenType.OperatorGreaterEquals, Operator.GreaterOrEqualComparison)]
    [InlineData(TokenType.OperatorEqualsEquals, Operator.EqualComparison)]
    [InlineData(TokenType.OperatorBangEquals, Operator.NotEqualComparison)]
    public void ComparisonExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        const string leftIdentifierName = "a";
        var leftToken = new Token(TokenType.Identifier, leftIdentifierName);
        var expectedLeftExpression = new IdentifierExpression(leftIdentifierName);

        var opToken = Helpers.GetDefaultToken(operatorTokenType);

        const string rightIdentifierName = "b";
        var rightToken = new Token(TokenType.Identifier, rightIdentifierName);
        var expectedRightExpression = new IdentifierExpression(rightIdentifierName);

        var lexerMock = new LexerMock(leftToken, opToken, rightToken, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var expression = expressionStatement.Expression.As<ComparisonExpression>();
        expression.Should().NotBeNull();
        expression!.Left.Should().BeEquivalentTo(expectedLeftExpression, Helpers.ProvideOptions);
        expression.Comparisons.Count.Should().Be(1);
        expression.Comparisons[0].Operator.Should().Be(expectedOperator);
        expression.Comparisons[0].Right.Should().BeEquivalentTo(expectedRightExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Binary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [InlineData(TokenType.KeywordOr, Operator.Disjunction)]
    [InlineData(TokenType.KeywordAnd, Operator.Conjunction)]
    public void BinaryPatternMatchingExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        const string leftIdentifierName = "b";
        const string rightIdentifierName = "c";

        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordMatch),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftBrace),
            new Token(TokenType.Identifier, leftIdentifierName),
            Helpers.GetDefaultToken(operatorTokenType),
            new Token(TokenType.Identifier, rightIdentifierName),
            Helpers.GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            Helpers.GetDefaultToken(TokenType.Semicolon),
            Helpers.GetDefaultToken(TokenType.RightBrace),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedLeftExpression = new FunctionCallExpression(new IdentifierExpression(leftIdentifierName),
            new List<Expression> { new IdentifierExpression("a") });
        var expectedRightExpression = new FunctionCallExpression(new IdentifierExpression(rightIdentifierName),
            new List<Expression> { new IdentifierExpression("a") });

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
        binaryExpression!.Left.Should().BeEquivalentTo(expectedLeftExpression, Helpers.ProvideOptions);
        binaryExpression.Operator.Should().Be(expectedOperator);
        binaryExpression.Right.Should().BeEquivalentTo(expectedRightExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);

        errorHandlerMock.HandledWarnings.Count.Should().Be(1);
        errorHandlerMock.HandledWarnings[0].Should().BeOfType<DefaultBranchMissing>();
    }

    [Trait("Category", "Binary expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(TokenType.OperatorDot)]
    [InlineData(TokenType.OperatorCaret)]
    [InlineData(TokenType.OperatorPlus)]
    [InlineData(TokenType.OperatorMinus)]
    [InlineData(TokenType.OperatorAsterisk)]
    [InlineData(TokenType.OperatorSlash)]
    [InlineData(TokenType.OperatorPercent)]
    [InlineData(TokenType.OperatorDotDot)]
    [InlineData(TokenType.OperatorLess)]
    [InlineData(TokenType.OperatorLessEquals)]
    [InlineData(TokenType.OperatorGreater)]
    [InlineData(TokenType.OperatorGreaterEquals)]
    [InlineData(TokenType.OperatorEqualsEquals)]
    [InlineData(TokenType.OperatorBangEquals)]
    [InlineData(TokenType.OperatorAndAnd)]
    [InlineData(TokenType.OperatorOrOr)]
    [InlineData(TokenType.OperatorQueryQuery)]
    [InlineData(TokenType.OperatorQueryGreater)]
    [InlineData(TokenType.OperatorEquals)]
    [InlineData(TokenType.OperatorPlusEquals)]
    [InlineData(TokenType.OperatorMinusEquals)]
    [InlineData(TokenType.OperatorAsteriskEquals)]
    [InlineData(TokenType.OperatorSlashEquals)]
    [InlineData(TokenType.OperatorPercentEquals)]
    public void MissingRightSideOfBinaryExpressionsShouldBeDetectedProperly(TokenType operatorTokenType)
    {
        const string leftIdentifierName = "a";

        var tokenSequence = new[]
        {
            new Token(TokenType.Identifier, leftIdentifierName),
            Helpers.GetDefaultToken(operatorTokenType),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new ExpectedExpression(new Position(2, 1, 2), TokenType.Semicolon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Binary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(TokenType.KeywordOr)]
    [InlineData(TokenType.KeywordAnd)]
    public void MissingRightSideOfBinaryPatternMatchingExpressionsShouldBeDetectedProperly(TokenType operatorTokenType)
    {
        const string leftIdentifierName = "b";

        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordMatch),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftBrace),
            new Token(TokenType.Identifier, leftIdentifierName),
            Helpers.GetDefaultToken(operatorTokenType),
            Helpers.GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            Helpers.GetDefaultToken(TokenType.Semicolon),
            Helpers.GetDefaultToken(TokenType.RightBrace),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new ExpectedExpression(new Position(7, 1, 7), TokenType.Colon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
