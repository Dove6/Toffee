using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTests
{
    [Trait("Category", "Variable initialization list statements")]
    [Theory]
    [InlineData("std")]
    [InlineData("std", "io")]
    [InlineData("one1", "two2", "three3")]
    public void NamespaceImportStatementsShouldBeParsedCorrectly(params string[] namespaceSegments)
    {
        var pullToken = Helpers.GetDefaultToken(TokenType.KeywordPull);

        var namespaceSegmentTokens = namespaceSegments.Select(x => new Token(TokenType.Identifier, x));
        var dotToken =Helpers.GetDefaultToken(TokenType.OperatorDot);
        var interleavedNamespaceSegments = namespaceSegmentTokens.SelectMany(x => new[] { x, dotToken })
            .Take(2 * namespaceSegments.Length - 1);

        var expectedNamespaceSegments = namespaceSegments.Select(x => new IdentifierExpression(x)).ToArray();

        var lexerMock = new LexerMock(interleavedNamespaceSegments.Prepend(pullToken).ToArray().AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var namespaceImportStatement = parser.CurrentStatement.As<NamespaceImportStatement>();
        namespaceImportStatement.Should().NotBeNull();
        namespaceImportStatement!.IsTerminated.Should().Be(true);
        namespaceImportStatement.NamespaceLevels.ToArray().Should().BeEquivalentTo(expectedNamespaceSegments, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Variable initialization list statements")]
    [Theory]
    [ClassData(typeof(VariableInitializationListStatementTestData))]
    public void VariableInitializationListStatementsShouldBeParsedCorrectly(Token[] tokenSequence, VariableInitialization[] expectedVariableList)
    {
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var variableInitializationStatement = parser.CurrentStatement.As<VariableInitializationListStatement>();
        variableInitializationStatement.Should().NotBeNull();
        variableInitializationStatement!.IsTerminated.Should().Be(true);
        variableInitializationStatement.Items.Should().BeEquivalentTo(expectedVariableList, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
