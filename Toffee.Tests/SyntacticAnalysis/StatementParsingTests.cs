using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTests
{
    [Trait("Category", "Comments")]
    [Fact]
    public void CommentsShouldBeIgnoredWhileParsing()
    {
        var initToken = Helpers.GetDefaultToken(TokenType.KeywordInit);
        var constToken = Helpers.GetDefaultToken(TokenType.KeywordConst);
        var identifierToken = new Token(TokenType.Identifier, "a");
        var equalToken = Helpers.GetDefaultToken(TokenType.OperatorEquals);
        var leftTermToken = new Token(TokenType.LiteralInteger, 123ul);
        var plusToken = Helpers.GetDefaultToken(TokenType.OperatorPlus);
        var rightTermToken = new Token(TokenType.LiteralFloat, 3.14);

        var expectedStatement = new VariableInitializationListStatement(new List<VariableInitialization>
        {
            new("a",
                new BinaryExpression(new LiteralExpression(DataType.Integer, 123ul),
                    Operator.Addition,
                    new LiteralExpression(DataType.Float, 3.14)),
                true)
        }) { IsTerminated = true };

        var comments = new[]
        {
            new Token(TokenType.LineComment, "line comment"),
            new Token(TokenType.BlockComment, "block comment")
        };

        var lexerMock =
            new LexerMock(
                new[] { initToken, constToken, identifierToken, equalToken, leftTermToken, plusToken, rightTermToken }
                    .SelectMany((x, i) => new[] { x, comments[i % 2] }).AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<VariableInitializationListStatement>();
        statement.Should().NotBeNull();
        statement.Should().BeEquivalentTo(expectedStatement, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }


    [Trait("Category", "Negative")]
    [Fact]
    public void UnterminatedTopLevelStatementsShouldBeDetectedProperly()
    {
        var tokenSequence = new[]
        {
            Helpers.GetDefaultToken(TokenType.KeywordBreak)
        };

        var expectedStatement = new BreakStatement();

        var expectedError = new ExpectedSemicolon(new Position(1, 1, 1), TokenType.EndOfText);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement).Should().BeTrue();

        var expressionStatement = statement.As<BreakStatement>();
        statement.Should().NotBeNull();
        statement.Should().BeEquivalentTo(expectedStatement, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
