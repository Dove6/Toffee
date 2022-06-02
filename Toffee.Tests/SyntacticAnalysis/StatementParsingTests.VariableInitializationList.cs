using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTest
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

        var lexerMock = new LexerMock(interleavedNamespaceSegments.Prepend(pullToken).ToArray());
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var namespaceImportStatement = parser.CurrentStatement.As<NamespaceImportStatement>();
        namespaceImportStatement.Should().NotBeNull();
        namespaceImportStatement!.IsTerminated.Should().Be(false);
        namespaceImportStatement.NamespaceLevels.ToArray().Should().BeEquivalentTo(expectedNamespaceSegments, Helpers.ProvideOptions);
    }

    [Trait("Category", "Variable initialization list statements")]
    [Theory]
    [ClassData(typeof(VariableInitializationListStatementTestData))]
    public void VariableInitializationListStatementsShouldBeParsedCorrectly(Token[] tokenSequence, VariableInitialization[] expectedVariableList)
    {
        var lexerMock = new LexerMock(tokenSequence);
        IParser parser = new Parser(lexerMock);

        parser.Advance();

        var returnStatement = parser.CurrentStatement.As<VariableInitializationListStatement>();
        returnStatement.Should().NotBeNull();
        returnStatement!.IsTerminated.Should().Be(false);
        returnStatement.Items.Should().BeEquivalentTo(expectedVariableList, Helpers.ProvideOptions);
    }
}
