using System;
using System.Collections.Generic;
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
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [ClassData(typeof(PatternMatchingExpressionTestData))]
    public void PatternMatchingExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedExpression, Type? expectedWarningType = null)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var wrappingBlock = expressionStatement.Expression.As<BlockExpression>();
        var patternMatchingExpression = wrappingBlock.ResultExpression.As<ConditionalExpression>();
        patternMatchingExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);

        if (expectedWarningType is not null)
        {
            errorHandlerMock.HandledWarnings.Count.Should().Be(1);
            errorHandlerMock.HandledWarnings[0].Should().BeOfType(expectedWarningType);
        }
        else
            Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Pattern matching expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(PatternMatchingExpressionMissingParenthesesTestData))]
    public void MissingParenthesesInPatternMatchingExpressionsShouldBeDetectedProperly(Token[] tokenSequence, Expression expectedExpression, ParserError[] expectedErrors, Type? expectedWarningType = null)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var wrappingBlock = expressionStatement.Expression.As<BlockExpression>();
        var patternMatchingExpression = wrappingBlock.ResultExpression.As<ConditionalExpression>();
        patternMatchingExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        for (var i = 0; i < expectedErrors.Length; i++)
            errorHandlerMock.HandledErrors[i].Should().BeEquivalentTo(expectedErrors[i]);

        if (expectedWarningType is not null)
        {
            errorHandlerMock.HandledWarnings.Count.Should().Be(1);
            errorHandlerMock.HandledWarnings[0].Should().BeOfType(expectedWarningType);
        }
        else
            Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Pattern matching expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(PatternMatchingSpecificationMissingColonOrSemicolonTestData))]
    public void MissingColonOrSemicolonInPatternMatchingSpecificationsShouldBeDetectedProperly(Token[] tokenSequence, Expression expectedExpression, ParserError expectedError, Type? expectedWarningType = null)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var wrappingBlock = expressionStatement.Expression.As<BlockExpression>();
        var patternMatchingExpression = wrappingBlock.ResultExpression.As<ConditionalExpression>();
        patternMatchingExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        if (expectedWarningType is not null)
        {
            errorHandlerMock.HandledWarnings.Count.Should().Be(1);
            errorHandlerMock.HandledWarnings[0].Should().BeOfType(expectedWarningType);
        }
        else
            Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Pattern matching expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(PatternMatchingBranchesMissingConsequent))]
    public void MissingConsequentOfPatternMatchingBranchesShouldBeDetectedProperly(Token[] tokenSequence, ParserError expectedError)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeFalse();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Pattern expressions")]
    [Trait("Category", "Grouping expressions")]
    [Trait("Category", "Negative")]
    [Fact]
    public void MissingClosingParenthesisInGroupingPatternExpressionsShouldBeDetectedProperly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordMatch),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.RightParenthesis),
            Helpers.GetDefaultToken(TokenType.LeftBrace),
            Helpers.GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "b"),
            Helpers.GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "c"),
            Helpers.GetDefaultToken(TokenType.Semicolon),
            Helpers.GetDefaultToken(TokenType.RightBrace),
            Helpers.GetDefaultToken(TokenType.Semicolon)
        };

        var expectedExpression = new ConditionalExpression(new List<ConditionalElement>
        {
            new(new GroupingExpression(new FunctionCallExpression(new IdentifierExpression("b"),
                    new List<Expression> { new IdentifierExpression("match") })),
                new BlockExpression(new List<Statement>(), new IdentifierExpression("c")))
        });

        var expectedError = new UnexpectedToken(new Position(7, 1, 7), TokenType.Colon, TokenType.RightParenthesis);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var wrappingBlock = expressionStatement.Expression.As<BlockExpression>();
        wrappingBlock.ResultExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        errorHandlerMock.HandledWarnings.Count.Should().Be(1);
        errorHandlerMock.HandledWarnings[0].Should().BeOfType<DefaultBranchMissing>();
    }

    // TODO: default being not last, occuring more than once, being missing
}
