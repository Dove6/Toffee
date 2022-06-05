using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Type check expressions")]
    [Theory]
    [InlineData(TokenType.KeywordInt, DataType.Integer)]
    [InlineData(TokenType.KeywordFloat, DataType.Float)]
    [InlineData(TokenType.KeywordString, DataType.String)]
    [InlineData(TokenType.KeywordBool, DataType.Bool)]
    [InlineData(TokenType.KeywordFunction, DataType.Function)]
    [InlineData(TokenType.KeywordNull, DataType.Null)]
    public void TypeInTypeCheckingExpressionsShouldBeParsedCorrectly(TokenType typeTokenType, DataType type)
    {
        var tokenSequence = new[]
        {
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.KeywordIs),
            Helpers.GetDefaultToken(typeTokenType),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedExpression = new TypeCheckExpression(new IdentifierExpression("a"), type);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Type check expressions")]
    [Trait("Category", "Binary expressions")]
    [Theory]
    [InlineData(new[] { TokenType.KeywordIs }, TokenType.KeywordInt, false, DataType.Integer)]
    [InlineData(new[] { TokenType.KeywordIs, TokenType.KeywordNot }, TokenType.KeywordNull, true, DataType.Null)]
    public void TypeCheckingBinaryExpressionsShouldBeParsedCorrectly(TokenType[] operatorTokenTypes, TokenType typeTokenType, bool expectedIsInequalityCheck, DataType expectedType)
    {
        const string leftIdentifierName = "a";
        var leftToken = new Token(TokenType.Identifier, leftIdentifierName);

        var opTokens = operatorTokenTypes.Select(Helpers.GetDefaultToken).ToArray();

        var rightToken = Helpers.GetDefaultToken(typeTokenType);
        var expectedExpression = new TypeCheckExpression(new IdentifierExpression(leftIdentifierName), expectedType,
            expectedIsInequalityCheck);

        var lexerMock = new LexerMock(opTokens.Prepend(leftToken).Append(rightToken).AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Type check expressions")]
    [Trait("Category", "Binary expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(new[] { TokenType.KeywordIs })]
    [InlineData(new[] { TokenType.KeywordIs, TokenType.KeywordNot })]
    public void MissingTypeInTypeCheckingBinaryExpressionsShouldBeDetectedProperly(TokenType[] operatorTokenTypes)
    {
        const string leftIdentifierName = "a";

        var tokenSequence = operatorTokenTypes.Select(Helpers.GetDefaultToken)
            .Prepend(new Token(TokenType.Identifier, leftIdentifierName))
            .AppendSemicolon();

        var errorPosition = (uint)operatorTokenTypes.Length + 1;
        var expectedError = new UnexpectedToken(new Position(errorPosition, 1, errorPosition), TokenType.Semicolon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should()
            .BeEquivalentTo(expectedError, o => o.Excluding(i => i.Name == "ExpectedType"));
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordInt);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordFloat);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordString);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordBool);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordFunction);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordNull);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Type check expressions")]
    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [InlineData(new[] { TokenType.KeywordIs }, TokenType.KeywordInt, false, DataType.Integer)]
    [InlineData(new[] { TokenType.KeywordIs, TokenType.KeywordNot }, TokenType.KeywordNull, true, DataType.Null)]
    public void TypeCheckingUnaryPatternMatchingExpressionsShouldBeParsedCorrectly(TokenType[] operatorTokenTypes, TokenType typeTokenType, bool expectedIsInequalityCheck, DataType expectedType)
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
            Helpers.GetDefaultToken(TokenType.RightBrace),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        }).ToArray();

        var expectedExpression = new PatternTypeCheckExpression(expectedType, expectedIsInequalityCheck);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Branches.Should().HaveCount(1);

        patternMatchingExpression.Branches[0].Pattern.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Type check expressions")]
    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(new[] { TokenType.KeywordIs })]
    [InlineData(new[] { TokenType.KeywordIs, TokenType.KeywordNot })]
    public void MissingTypeInTypeCheckingUnaryPatternMatchingExpressionsShouldBeDetectedProperly(TokenType[] operatorTokenTypes)
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
            Helpers.GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            Helpers.GetDefaultToken(TokenType.Semicolon),
            Helpers.GetDefaultToken(TokenType.RightBrace),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        }).ToArray();

        var errorPosition = (uint)operatorTokenTypes.Length + 5;
        var expectedError = new UnexpectedToken(new Position(errorPosition, 1, errorPosition), TokenType.Colon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should()
            .BeEquivalentTo(expectedError, o => o.Excluding(i => i.Name == "ExpectedType"));
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordInt);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordFloat);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordString);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordBool);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordFunction);
        errorHandlerMock.HandledErrors[0].As<UnexpectedToken>().ExpectedType.Should().Contain(TokenType.KeywordNull);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
