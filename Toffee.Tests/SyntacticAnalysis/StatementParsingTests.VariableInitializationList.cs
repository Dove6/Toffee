using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTests
{
    [Trait("Category", "Variable initialization list statements")]
    [Theory]
    [ClassData(typeof(VariableInitializationListStatementTestData))]
    public void VariableInitializationListStatementsShouldBeParsedCorrectly(Token[] tokenSequence, VariableInitialization[] expectedVariableList)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
        hadError.Should().BeFalse();

        var variableInitializationStatement = statement.As<VariableInitializationListStatement>();
        variableInitializationStatement.Should().NotBeNull();
        variableInitializationStatement!.IsTerminated.Should().Be(true);
        variableInitializationStatement.Items.Should().BeEquivalentTo(expectedVariableList, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Variable initialization list statements")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(VariableInitializationListStatementMissingVariableTestData))]
    public void MissingVariableInVariableInitializationListStatementsShouldBeDetectedProperly(Token[] tokenSequence, params ParserError[] expectedErrors)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out _, out var hadError);
        hadError.Should().BeTrue();

        for (var i = 0; i < expectedErrors.Length; i++)
            errorHandlerMock.HandledErrors[i].Should().BeEquivalentTo(expectedErrors[i]);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Variable initialization list statements")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingInitialValueInVariableInitializationListStatementsShouldBeDetectedProperly(bool constKeywordUsed)
    {
        var tokenSequence = (constKeywordUsed
            ? new[] { Helpers.GetDefaultToken(TokenType.KeywordInit), Helpers.GetDefaultToken(TokenType.KeywordConst) }
            : new[] { Helpers.GetDefaultToken(TokenType.KeywordInit) })
            .Concat(new[]
            {
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.OperatorEquals)
            })
            .AppendSemicolon();

        var errorPosition = 3u + (constKeywordUsed ? 1u : 0u);
        var expectedError = new ExpectedExpression(new Position(errorPosition, 1, errorPosition), TokenType.Semicolon);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeTrue();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Variable initialization list statements")]
    [Trait("Category", "Negative")]
    [Theory]
    [InlineData(TokenType.OperatorEqualsEquals, true)]
    [InlineData(TokenType.OperatorPlusEquals, true)]
    [InlineData(TokenType.OperatorMinusEquals, true)]
    [InlineData(TokenType.OperatorAsteriskEquals, true)]
    [InlineData(TokenType.OperatorSlashEquals, true)]
    [InlineData(TokenType.OperatorPercentEquals, true)]
    [InlineData(TokenType.OperatorEqualsEquals, false)]
    [InlineData(TokenType.OperatorPlusEquals, false)]
    [InlineData(TokenType.OperatorMinusEquals, false)]
    [InlineData(TokenType.OperatorAsteriskEquals, false)]
    [InlineData(TokenType.OperatorSlashEquals, false)]
    [InlineData(TokenType.OperatorPercentEquals, false)]
    public void BadAssignmentOperatorInVariableInitializationListStatementsShouldBeDetectedProperly(TokenType operatorTokenType, bool constKeywordUsed)
    {
        const string initializedVariableName = "a";

        var tokenSequence = (constKeywordUsed
                ? new[] { Helpers.GetDefaultToken(TokenType.KeywordInit), Helpers.GetDefaultToken(TokenType.KeywordConst) }
                : new[] { Helpers.GetDefaultToken(TokenType.KeywordInit) })
            .Concat(new[]
            {
                new Token(TokenType.Identifier, initializedVariableName),
                Helpers.GetDefaultToken(operatorTokenType),
                new Token(TokenType.LiteralInteger, 123ul)
            })
            .AppendSemicolon();

        var expectedStatement = new VariableInitializationListStatement(new List<VariableInitialization>
        {
            new(initializedVariableName, new LiteralExpression(DataType.Integer, 123ul), constKeywordUsed)
        }) { IsTerminated = true };

        var errorPosition = 2u + (constKeywordUsed ? 1u : 0u);
        var expectedError = new UnexpectedToken(new Position(errorPosition, 1, errorPosition),
            operatorTokenType, TokenType.OperatorEquals);

        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
        hadError.Should().BeTrue();

        var variableInitializationStatement = statement.As<VariableInitializationListStatement>();
        variableInitializationStatement.Should().BeEquivalentTo(expectedStatement, Helpers.ProvideOptions);

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
