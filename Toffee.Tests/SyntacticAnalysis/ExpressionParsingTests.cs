using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis.Generators;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ExpressionParsingTest
{
    // TODO: negative tests

    [Trait("Category", "Nesting")]
    [Fact]
    public void NestedExpressionsShouldBeHandledCorrectly()
    {
        var tokenSequence = new[]
        {
            new Token(TokenType.Identifier, "a"),
            Helpers.GetDefaultToken(TokenType.OperatorEquals),
            new Token(TokenType.Identifier, "b"),
            Helpers.GetDefaultToken(TokenType.OperatorEquals),
            new Token(TokenType.LiteralInteger, 5L),
            Helpers.GetDefaultToken(TokenType.OperatorPlus),
            Helpers.GetDefaultToken(TokenType.OperatorMinus),
            Helpers.GetDefaultToken(TokenType.OperatorBang),
            Helpers.GetDefaultToken(TokenType.KeywordFalse)
        };

        var expectedTree = new BinaryExpression(
            new IdentifierExpression("a"),
            Operator.Assignment,
            new BinaryExpression(
                new IdentifierExpression("b"),
                Operator.Assignment,
                new BinaryExpression(
                    new LiteralExpression(DataType.Integer, 5L),
                    Operator.Addition,
                    new UnaryExpression(
                        Operator.ArithmeticNegation,
                        new UnaryExpression(
                            Operator.LogicalNegation,
                            new LiteralExpression(DataType.Bool, false))))));

        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedTree, Helpers.ProvideOptions);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Associativity")]
    [Trait("Category", "Nesting")]
    [Theory]
    [ClassData(typeof(OperatorsAssociativityTestData))]
    public void ExpressionsShouldBeParsedWithRespectToOperatorsAssociativity(Token[] tokenSequence, Expression expectedExpression)
    {
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
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

    [Trait("Category", "Priority")]
    [Trait("Category", "Nesting")]
    [Theory]
    [ClassData(typeof(OperatorsPriorityTestData))]
    public void ExpressionsShouldBeParsedWithRespectToOperatorsPriority(Token[] tokenSequence, Expression expectedExpression, bool shouldIgnoreErrors = false)
    {
        var lexerMock = new LexerMock(tokenSequence.AppendSemicolon());
        var errorHandlerMock = new ParserErrorHandlerMock();
        IParser parser = new Parser(lexerMock, errorHandlerMock);

        parser.Advance();

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(true);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, Helpers.ProvideOptions);

        if (!shouldIgnoreErrors)
            Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    // TODO: test if position of a token equals position of its first lexeme
}
