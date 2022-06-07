using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class StatementParsingTests
{
    [Trait("Category", "Namespace import statements")]
    [Theory]
    [InlineData("std")]
    [InlineData("std", "io")]
    [InlineData("one1", "two2", "three3")]
    public void NamespaceImportStatementsShouldBeParsedCorrectly(params string[] namespaceSegments)
    {
        var pullToken = Helpers.GetDefaultToken(TokenType.KeywordPull);

        var namespaceSegmentTokens = namespaceSegments.Select(x => new Token(TokenType.Identifier, x));
        var dotToken = Helpers.GetDefaultToken(TokenType.OperatorDot);
        var interleavedNamespaceSegments = namespaceSegmentTokens.SelectMany(x => new[] { x, dotToken })
            .Take(2 * namespaceSegments.Length - 1);

        var lexerMock = new LexerMock(interleavedNamespaceSegments.Prepend(pullToken).AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeFalse();

        var namespaceImportStatement = statement.As<NamespaceImportStatement>();
        namespaceImportStatement.Should().NotBeNull();
        namespaceImportStatement!.IsTerminated.Should().Be(true);
        namespaceImportStatement.NamespaceLevels.ToArray().Should().BeEquivalentTo(namespaceSegments, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Namespace import statements")]
    [Trait("Category", "Negative")]
    [Theory]
    [ClassData(typeof(NamespaceImportStatementNonIdentifiersTestData))]
    public void NonIdentifiersInNamespaceImportStatementsShouldBeDetectedProperly(Token[] tokenSequence, ParserError expectedError)
    {
        var lexerMock = new LexerMock(tokenSequence);
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.TryAdvance(out var statement, out var hadError);
hadError.Should().BeTrue();

        errorHandlerMock.HandledErrors[0].Should().BeEquivalentTo(expectedError);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
