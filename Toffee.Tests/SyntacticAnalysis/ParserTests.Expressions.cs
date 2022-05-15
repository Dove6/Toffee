using System.Linq;
using FluentAssertions;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ParserTests
{
    [Trait("Category", "Literal expressions")]
    [Theory]
    [InlineData(TokenType.LiteralInteger, 1234L, DataType.Integer, 1234L)]
    [InlineData(TokenType.LiteralFloat, 3.14, DataType.Float, 3.14)]
    [InlineData(TokenType.LiteralString, "abcd", DataType.String, "abcd")]
    [InlineData(TokenType.KeywordTrue, "true", DataType.Bool, true)]
    [InlineData(TokenType.KeywordFalse, "false", DataType.Bool, false)]
    [InlineData(TokenType.KeywordNull, null, DataType.Null, null)]
    public void LiteralExpressionsShouldBeParsedCorrectly(TokenType literalTokenType, object? literalTokenContent, DataType literalType, object? literalValue)
    {
        var literalToken = new Token(literalTokenType, literalTokenContent);

        var lexerMock = new LexerMock(literalToken);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var expression = expressionStatement.Expression.As<LiteralExpression>();
        expression.Should().NotBeNull();
        expression!.Type.Should().Be(literalType);
        expression.Value.Should().Be(literalValue);
    }

    [Trait("Category", "Identifier expressions")]
    [Fact]
    public void IdentifierExpressionsShouldBeParsedCorrectly()
    {
        const string identifierName = "ident";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var lexerMock = new LexerMock(identifierToken);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var expression = expressionStatement.Expression.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression!.Name.Should().Be(identifierName);
    }

    [Trait("Category", "Type expressions")]
    [Theory(Skip = /* TODO: */ "types cannot function as standalone expressions and should be tested alongside casts")]
    [InlineData(TokenType.KeywordInt, DataType.Integer)]
    [InlineData(TokenType.KeywordFloat, DataType.Float)]
    [InlineData(TokenType.KeywordString, DataType.String)]
    [InlineData(TokenType.KeywordBool, DataType.Bool)]
    [InlineData(TokenType.KeywordFunction, DataType.Function)]
    [InlineData(TokenType.KeywordNull, DataType.Null)]
    public void TypeExpressionsShouldBeParsedCorrectly(TokenType typeTokenType, DataType type)
    {
        var typeToken = new Token(typeTokenType, MapTokenTypeToContent(typeTokenType));

        var lexerMock = new LexerMock(typeToken);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var expression = expressionStatement.Expression.As<TypeExpression>();
        expression.Should().NotBeNull();
        expression!.Type.Should().Be(type);
    }

    [Trait("Category", "Binary expressions")]
    [Theory]
    [InlineData(TokenType.OperatorDot, Operator.NamespaceAccess)]
    [InlineData(TokenType.OperatorCaret, Operator.Exponentiation)]
    [InlineData(TokenType.OperatorPlus, Operator.Addition)]
    [InlineData(TokenType.OperatorMinus, Operator.Subtraction)]
    [InlineData(TokenType.OperatorAsterisk, Operator.Multiplication)]
    [InlineData(TokenType.OperatorSlash, Operator.Division)]
    [InlineData(TokenType.OperatorPercent, Operator.Remainder)]
    [InlineData(TokenType.OperatorDotDot, Operator.Concatenation)]
    [InlineData(TokenType.OperatorLess, Operator.LessThanComparison)]
    [InlineData(TokenType.OperatorLessEquals, Operator.LessOrEqualComparison)]
    [InlineData(TokenType.OperatorGreater, Operator.GreaterThanComparison)]
    [InlineData(TokenType.OperatorGreaterEquals, Operator.GreaterOrEqualComparison)]
    [InlineData(TokenType.OperatorEqualsEquals, Operator.EqualComparison)]
    [InlineData(TokenType.OperatorBangEquals, Operator.NotEqualComparison)]
    [InlineData(TokenType.OperatorAndAnd, Operator.Conjunction)]
    [InlineData(TokenType.OperatorOrOr, Operator.Disjunction)]
    [InlineData(TokenType.OperatorQueryQuery, Operator.NullCoalescing)]
    [InlineData(TokenType.OperatorQueryGreater, Operator.NullSafePipe)]
    [InlineData(TokenType.OperatorEquals, Operator.Assignment)]
    [InlineData(TokenType.OperatorPlusEquals, Operator.AdditionAssignment)]
    [InlineData(TokenType.OperatorMinusEquals, Operator.SubtractionAssignment)]
    [InlineData(TokenType.OperatorAsteriskEquals, Operator.MultiplicationAssignment)]
    [InlineData(TokenType.OperatorSlashEquals, Operator.DivisionAssignment)]
    [InlineData(TokenType.OperatorPercentEquals, Operator.RemainderAssignment)]
    public void BinaryExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        const string leftIdentifierName = "a";
        var leftToken = new Token(TokenType.Identifier, leftIdentifierName);
        var expectedLeftExpression = new IdentifierExpression(leftIdentifierName);

        var opToken = new Token(operatorTokenType, MapTokenTypeToContent(operatorTokenType));

        const string rightIdentifierName = "b";
        var rightToken = new Token(TokenType.Identifier, rightIdentifierName);
        var expectedRightExpression = new IdentifierExpression(rightIdentifierName);

        var lexerMock = new LexerMock(leftToken, opToken, rightToken);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var expression = expressionStatement.Expression.As<BinaryExpression>();
        expression.Should().NotBeNull();
        expression!.Left.Should().BeEquivalentTo(expectedLeftExpression, ProvideOptions);
        expression.Operator.Should().Be(expectedOperator);
        expression.Right.Should().BeEquivalentTo(expectedRightExpression, ProvideOptions);
    }

    [Trait("Category", "Unary expressions")]
    [Theory]
    [InlineData(TokenType.OperatorPlus, Operator.NumberPromotion)]
    [InlineData(TokenType.OperatorMinus, Operator.ArithmeticNegation)]
    [InlineData(TokenType.OperatorBang, Operator.LogicalNegation)]
    public void UnaryExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        var opToken = new Token(operatorTokenType, MapTokenTypeToContent(operatorTokenType));

        const string identifierName = "a";
        var token = new Token(TokenType.Identifier, identifierName);
        var expectedExpression = new IdentifierExpression(identifierName);

        var lexerMock = new LexerMock(opToken, token);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var expression = expressionStatement.Expression.As<UnaryExpression>();
        expression.Should().NotBeNull();
        expression!.Expression.Should().BeEquivalentTo(expectedExpression, ProvideOptions);
        expression.Operator.Should().Be(expectedOperator);
    }

    [Trait("Category", "Binary expressions")]
    [Theory]
    [InlineData(new[] { TokenType.KeywordIs }, TokenType.KeywordInt, Operator.EqualTypeCheck, DataType.Integer)]
    [InlineData(new[] { TokenType.KeywordIs, TokenType.KeywordNot }, TokenType.KeywordNull, Operator.NotEqualTypeCheck, DataType.Null)]
    public void TypeCheckingBinaryExpressionsShouldBeParsedCorrectly(TokenType[] operatorTokenTypes, TokenType typeTokenType, Operator expectedOperator, DataType expectedType)
    {
        const string leftIdentifierName = "a";
        var leftToken = new Token(TokenType.Identifier, leftIdentifierName);
        var expectedLeftExpression = new IdentifierExpression(leftIdentifierName);

        var opTokens = operatorTokenTypes.Select(x => new Token(x, MapTokenTypeToContent(x))).ToArray();

        var rightToken = new Token(typeTokenType, MapTokenTypeToContent(typeTokenType));
        var expectedRightExpression = new TypeExpression(expectedType);

        var lexerMock = new LexerMock(opTokens.Prepend(leftToken).Append(rightToken).ToArray());

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var expression = expressionStatement.Expression.As<BinaryExpression>();
        expression.Should().NotBeNull();
        expression!.Left.Should().BeEquivalentTo(expectedLeftExpression, ProvideOptions);
        expression.Operator.Should().Be(expectedOperator);
        expression.Right.Should().BeEquivalentTo(expectedRightExpression, ProvideOptions);
    }
}
