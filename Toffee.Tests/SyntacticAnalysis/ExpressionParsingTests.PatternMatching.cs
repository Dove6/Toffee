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
    public void PatternMatchingExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedArgument, PatternMatchingBranch[] expectedBranches, Expression? expectedDefault)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Argument.Should().BeEquivalentTo(expectedArgument, Helpers.ProvideOptions);
        patternMatchingExpression.Branches.ToArray().Should().BeEquivalentTo(expectedBranches, Helpers.ProvideOptions);
        patternMatchingExpression.Default.Should().BeEquivalentTo(expectedDefault, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Pattern matching expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(PatternMatchingExpressionMissingParenthesesTestData))]
    public void MissingParenthesesInPatternMatchingExpressionsShouldBeDetectedProperly(Token[] tokenSequence, Expression expectedExpression, params ParserError[] expectedErrors)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        for (var i = 0; i < expectedErrors.Length; i++)
            errorHandlerMock.HandledErrors[i].Should().BeEquivalentTo(expectedErrors[i]);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Pattern matching expressions")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(PatternMatchingSpecificationMissingColonOrSemicolonTestData))]
    public void MissingColonOrSemicolonInPatternMatchingSpecificationsShouldBeDetectedProperly(Token[] tokenSequence, Expression expectedExpression, ParserError expectedError)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

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

        parser.Advance();

        parser.CurrentStatement.Should().BeNull();

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

        var expectedExpression = new PatternMatchingExpression(new IdentifierExpression("a"),
            new List<PatternMatchingBranch> { new(new GroupingExpression(new IdentifierExpression("b")),
                new IdentifierExpression("c")) });

        var expectedError = new UnexpectedToken(new Position(7, 1, 7), TokenType.Colon, TokenType.RightParenthesis);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
