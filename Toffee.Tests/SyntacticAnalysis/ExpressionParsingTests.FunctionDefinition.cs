using System;
using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Function definition expressions")]
    [Theory]
    [ClassData(typeof(FunctionDefinitionExpressionTestData))]
    public void FunctionDefinitionExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, FunctionParameter[] expectedParameters, BlockExpression expectedBody)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var functionDefinitionExpression = expressionStatement.Expression.As<FunctionDefinitionExpression>();
        functionDefinitionExpression.Should().NotBeNull();
        functionDefinitionExpression.Parameters.ToArray().Should().BeEquivalentTo(expectedParameters, Helpers.ProvideOptions);
        functionDefinitionExpression.Body.Should().BeEquivalentTo(expectedBody, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Function definition expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(FunctionDefinitionExpressionMissingParenthesisTestData))]
    public void MissingParenthesesInFunctionDefinitionParameterListShouldBeDetectedProperly(Token[] tokenSequence, Expression expectedExpression, params ParserError[] expectedErrors)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var functionDefinitionExpression = expressionStatement.Expression.As<FunctionDefinitionExpression>();
        functionDefinitionExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        for (var i = 0; i < expectedErrors.Length; i++)
            errorHandlerMock.HandledErrors[i].Should().BeEquivalentTo(expectedErrors[i]);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Function definition expressions")]
    [Trait("Category", "Negative")]
    [Fact]
    public void MissingBodyOfFunctionDefinitionParameterListShouldBeDetectedProperly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordFuncti),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedError = new ExpectedBlockExpression(new Position(3, 1, 3), TokenType.Semicolon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Function definition expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void MissingParameterInFunctionDefinitionExpressionsShouldBeDetectedProperly(bool missingAfterComma, bool missingIsConst, bool presentIsConst)
    {
        var missingParameter = missingIsConst ? new[] { Helpers.GetDefaultToken(TokenType.KeywordConst) } : Array.Empty<Token>();
        var presentParameter = presentIsConst
            ? new[] { Helpers.GetDefaultToken(TokenType.KeywordConst), new Token(TokenType.Identifier, "a") }
            : new[] { new Token(TokenType.Identifier, "a") };
        var firstParameter = missingAfterComma ? presentParameter : missingParameter;
        var secondParameter = missingAfterComma ? missingParameter : presentParameter;
        var tokenSequence = new[]
            {
                Helpers.GetDefaultToken(TokenType.KeywordFuncti),
                Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            }.Concat(firstParameter)
            .Append(Helpers.GetDefaultToken(TokenType.Comma))
            .Concat(secondParameter)
            .Concat(new[]
            {
                Helpers.GetDefaultToken(TokenType.RightParenthesis),
                Helpers.GetDefaultToken(TokenType.LeftBrace),
                Helpers.GetDefaultToken(TokenType.RightBrace)
            })
            .AppendSemicolon();

        var errorPosition = 2u;
        if (missingAfterComma)
            errorPosition += 2u + (missingIsConst ? 1u : 0u) + (presentIsConst ? 1u : 0u);
        else
            errorPosition += missingIsConst ? 1u : 0u;
        var expectedError = new ExpectedExpression(new Position(errorPosition, 1, errorPosition),
            missingAfterComma ? TokenType.RightParenthesis : TokenType.Comma);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
