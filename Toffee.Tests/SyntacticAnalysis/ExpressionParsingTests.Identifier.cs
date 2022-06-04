using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    [Trait("Category", "Identifier expressions")]
    [Fact]
    public void IdentifierExpressionsShouldBeParsedCorrectly()
    {
        const string identifierName = "ident";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var lexerMock = new LexerMock(identifierToken, Helpers.GetDefaultToken(TokenType.Semicolon));
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        var expression = expressionStatement.Expression.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression!.Name.Should().Be(identifierName);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
