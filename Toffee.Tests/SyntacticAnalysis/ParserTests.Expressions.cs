using System;
using System.Collections.Generic;
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
        var typeToken = GetDefaultToken(typeTokenType);

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

        var opToken = GetDefaultToken(operatorTokenType);

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
        var opToken = GetDefaultToken(operatorTokenType);

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

        var opTokens = operatorTokenTypes.Select(GetDefaultToken).ToArray();

        var rightToken = GetDefaultToken(typeTokenType);
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

    [Trait("Category", "Block expressions")]
    [Theory]
    [MemberData(nameof(GenerateBlockExpressionTestData))]
    public void BlockExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Statement[] expectedStatementList, Statement? expectedUnterminatedStatement)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var blockExpression = expressionStatement.Expression.As<BlockExpression>();
        blockExpression.Should().NotBeNull();
        blockExpression!.Statements.ToArray().Should().BeEquivalentTo(expectedStatementList, ProvideOptions);
        blockExpression.UnterminatedStatement.Should().BeEquivalentTo(expectedUnterminatedStatement, ProvideOptions);
    }

    [Trait("Category", "Conditional expressions")]
    [Theory]
    [MemberData(nameof(GenerateConditionalExpressionTestData))]
    public void ConditionalExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, ConditionalElement expectedIfPart, ConditionalElement[] expectedElifParts, Expression? expectedElsePart)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var conditionalExpression = expressionStatement.Expression.As<ConditionalExpression>();
        conditionalExpression.Should().NotBeNull();
        conditionalExpression.IfPart.Should().BeEquivalentTo(expectedIfPart, ProvideOptions);
        conditionalExpression.ElifParts.ToArray().Should().BeEquivalentTo(expectedElifParts, ProvideOptions);
        conditionalExpression.ElsePart.Should().BeEquivalentTo(expectedElsePart, ProvideOptions);
    }

    [Trait("Category", "For loop expressions")]
    [Theory]
    [MemberData(nameof(GenerateForLoopExpressionTestData))]
    public void ForLoopExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, string? expectedCounterName, ForLoopRange expectedRange, Expression expectedBody)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var forLoopExpression = expressionStatement.Expression.As<ForLoopExpression>();
        forLoopExpression.Should().NotBeNull();
        forLoopExpression.CounterName.Should().Be(expectedCounterName);
        forLoopExpression.Range.Should().BeEquivalentTo(expectedRange, ProvideOptions);
        forLoopExpression.Body.Should().BeEquivalentTo(expectedBody, ProvideOptions);
    }

    [Trait("Category", "While loop expressions")]
    [Theory]
    [MemberData(nameof(GenerateWhileLoopExpressionTestData))]
    public void WhileLoopExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedCondition, Expression expectedBody)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var whileLoopExpression = expressionStatement.Expression.As<WhileLoopExpression>();
        whileLoopExpression.Should().NotBeNull();
        whileLoopExpression.Condition.Should().BeEquivalentTo(expectedCondition, ProvideOptions);
        whileLoopExpression.Body.Should().BeEquivalentTo(expectedBody, ProvideOptions);
    }

    [Trait("Category", "Function definition expressions")]
    [Theory]
    [MemberData(nameof(GenerateFunctionDefinitionExpressionTestData))]
    public void FunctionDefinitionExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, FunctionParameter[] expectedParameters, BlockExpression expectedBody)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var functionDefinitionExpression = expressionStatement.Expression.As<FunctionDefinitionExpression>();
        functionDefinitionExpression.Should().NotBeNull();
        functionDefinitionExpression.Parameters.ToArray().Should().BeEquivalentTo(expectedParameters, ProvideOptions);
        functionDefinitionExpression.Body.Should().BeEquivalentTo(expectedBody, ProvideOptions);
    }

    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [MemberData(nameof(GeneratePatternMatchingExpressionTestData))]
    public void PatternMatchingExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedArgument, PatternMatchingBranch[] expectedBranches, Expression? expectedDefault)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Argument.Should().BeEquivalentTo(expectedArgument, ProvideOptions);
        patternMatchingExpression.Branches.ToArray().Should().BeEquivalentTo(expectedBranches, ProvideOptions);
        patternMatchingExpression.Default.Should().BeEquivalentTo(expectedDefault, ProvideOptions);
    }

    [Trait("Category", "Function call expressions")]
    [Theory]
    [MemberData(nameof(GenerateFunctionCallExpressionTestData))]
    public void FunctionCallExpressionsShouldBeParsedCorrectly(Token[] tokenSequence, Expression expectedCalledExpression, Expression[] expectedArguments)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var functionCallExpression = expressionStatement.Expression.As<FunctionCallExpression>();
        functionCallExpression.Should().NotBeNull();
        functionCallExpression.Expression.Should().BeEquivalentTo(expectedCalledExpression, ProvideOptions);
        functionCallExpression.Arguments.ToArray().Should().BeEquivalentTo(expectedArguments, ProvideOptions);
    }

    [Trait("Category", "Binary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [InlineData(TokenType.KeywordOr, Operator.PatternMatchingDisjunction)]
    [InlineData(TokenType.KeywordAnd, Operator.PatternMatchingConjunction)]
    public void BinaryPatternMatchingExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        const string leftIdentifierName = "b";
        const string rightIdentifierName = "c";

        var tokenSequence = new[]
        {
            GetDefaultToken(TokenType.KeywordMatch),
            GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            GetDefaultToken(TokenType.RightParenthesis),
            GetDefaultToken(TokenType.LeftBrace),
            new Token(TokenType.Identifier, leftIdentifierName),
            GetDefaultToken(operatorTokenType),
            new Token(TokenType.Identifier, rightIdentifierName),
            GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            GetDefaultToken(TokenType.Semicolon),
            GetDefaultToken(TokenType.RightBrace)
        };

        var expectedLeftExpression = new IdentifierExpression(leftIdentifierName);
        var expectedRightExpression = new IdentifierExpression(rightIdentifierName);

        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Branches.Should().HaveCount(1);

        var binaryExpression = patternMatchingExpression.Branches[0].Pattern.As<BinaryExpression>();
        binaryExpression.Should().NotBeNull();
        binaryExpression!.Left.Should().BeEquivalentTo(expectedLeftExpression, ProvideOptions);
        binaryExpression.Operator.Should().Be(expectedOperator);
        binaryExpression.Right.Should().BeEquivalentTo(expectedRightExpression, ProvideOptions);
    }

    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [InlineData(TokenType.OperatorLess, Operator.PatternMatchingLessThanComparison)]
    [InlineData(TokenType.OperatorLessEquals, Operator.PatternMatchingLessOrEqualComparison)]
    [InlineData(TokenType.OperatorGreater, Operator.PatternMatchingGreaterThanComparison)]
    [InlineData(TokenType.OperatorGreaterEquals, Operator.PatternMatchingGreaterOrEqualComparison)]
    [InlineData(TokenType.OperatorEqualsEquals, Operator.PatternMatchingEqualComparison)]
    [InlineData(TokenType.OperatorBangEquals, Operator.PatternMatchingNotEqualComparison)]
    public void UnaryPatternMatchingExpressionsShouldBeParsedCorrectly(TokenType operatorTokenType, Operator expectedOperator)
    {
        var tokenSequence = new[]
        {
            GetDefaultToken(TokenType.KeywordMatch),
            GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            GetDefaultToken(TokenType.RightParenthesis),
            GetDefaultToken(TokenType.LeftBrace),
            GetDefaultToken(operatorTokenType),
            new Token(TokenType.LiteralInteger, 5L),
            GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            GetDefaultToken(TokenType.Semicolon),
            GetDefaultToken(TokenType.RightBrace)
        };

        var expectedExpression = new LiteralExpression(DataType.Integer, 5L);

        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Branches.Should().HaveCount(1);

        var unaryExpression = patternMatchingExpression.Branches[0].Pattern.As<UnaryExpression>();
        unaryExpression.Should().NotBeNull();
        unaryExpression.Operator.Should().Be(expectedOperator);
        unaryExpression.Expression.Should().BeEquivalentTo(expectedExpression, ProvideOptions);
    }

    [Trait("Category", "Unary expressions")]
    [Trait("Category", "Pattern matching expressions")]
    [Theory]
    [InlineData(new[] { TokenType.KeywordIs }, TokenType.KeywordInt, Operator.PatternMatchingEqualTypeCheck, DataType.Integer)]
    [InlineData(new[] { TokenType.KeywordIs, TokenType.KeywordNot }, TokenType.KeywordNull, Operator.PatternMatchingNotEqualTypeCheck, DataType.Null)]
    public void TypeCheckingUnaryPatternMatchingExpressionsShouldBeParsedCorrectly(TokenType[] operatorTokenTypes, TokenType typeTokenType, Operator expectedOperator, DataType expectedType)
    {
        var tokenSequence = new[]
        {
            GetDefaultToken(TokenType.KeywordMatch),
            GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            GetDefaultToken(TokenType.RightParenthesis),
            GetDefaultToken(TokenType.LeftBrace)
        }.Concat(operatorTokenTypes.Select(GetDefaultToken)).Concat(new[]
        {
            GetDefaultToken(typeTokenType),
            GetDefaultToken(TokenType.Colon),
            new Token(TokenType.Identifier, "d"),
            GetDefaultToken(TokenType.Semicolon),
            GetDefaultToken(TokenType.RightBrace)
        }).ToArray();

        var expectedExpression = new TypeExpression(expectedType);

        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        var patternMatchingExpression = expressionStatement.Expression.As<PatternMatchingExpression>();
        patternMatchingExpression.Should().NotBeNull();
        patternMatchingExpression.Branches.Should().HaveCount(1);

        var unaryExpression = patternMatchingExpression.Branches[0].Pattern.As<UnaryExpression>();
        unaryExpression.Should().NotBeNull();
        unaryExpression.Operator.Should().Be(expectedOperator);
        unaryExpression.Expression.Should().BeEquivalentTo(expectedExpression, ProvideOptions);
    }

    [Trait("Category", "Nesting")]
    [Fact]
    public void NestedExpressionsShouldBeHandledCorrectly()
    {
        var tokenSequence = new[]
        {
            new Token(TokenType.Identifier, "a"),
            GetDefaultToken(TokenType.OperatorEquals),
            new Token(TokenType.Identifier, "b"),
            GetDefaultToken(TokenType.OperatorEquals),
            new Token(TokenType.LiteralInteger, 5L),
            GetDefaultToken(TokenType.OperatorPlus),
            GetDefaultToken(TokenType.OperatorMinus),
            GetDefaultToken(TokenType.OperatorBang),
            GetDefaultToken(TokenType.KeywordFalse)
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

        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedTree, ProvideOptions);
    }

    [Trait("Category", "Associativity")]
    [Trait("Category", "Nesting")]
    [Theory]
    [MemberData(nameof(GenerateOperatorsAssociativityTestData))]
    public void ExpressionsShouldBeParsedWithRespectToOperatorsAssociativity(Token[] tokenSequence, Expression expectedExpression)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, ProvideOptions);
    }

    [Trait("Category", "Priority")]
    [Trait("Category", "Nesting")]
    [Theory]
    [MemberData(nameof(GenerateOperatorsPriorityTestData))]
    public void ExpressionsShouldBeParsedWithRespectToOperatorsPriority(Token[] tokenSequence, Expression expectedExpression)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, ProvideOptions);
    }

    [Trait("Category", "Grouping expressions")]
    [Trait("Category", "Nesting")]
    [Fact]
    public void GroupingExpressionsShouldBeParsedCorrectly()
    {
        var tokenSequence = new[]
        {
            GetDefaultToken(TokenType.LeftParenthesis),
            GetDefaultToken(TokenType.LeftParenthesis),
            GetDefaultToken(TokenType.LeftParenthesis),
            new Token(TokenType.Identifier, "a"),
            GetDefaultToken(TokenType.RightParenthesis),
            GetDefaultToken(TokenType.OperatorPlus),
            new Token(TokenType.LiteralInteger, 5L),
            GetDefaultToken(TokenType.RightParenthesis),
            GetDefaultToken(TokenType.RightParenthesis)
        };

        var expectedExpression = new GroupingExpression(new GroupingExpression(new BinaryExpression(
            new GroupingExpression(new IdentifierExpression("a")),
            Operator.Addition,
            new LiteralExpression(DataType.Integer, 5L))));

        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);

        expressionStatement.Expression.Should().BeEquivalentTo(expectedExpression, ProvideOptions);
    }

    #region Generators

    public static IEnumerable<object[]> GenerateBlockExpressionTestData()
    {
        var leftBraceToken = GetDefaultToken(TokenType.LeftBrace);
        var rightBraceToken = GetDefaultToken(TokenType.RightBrace);
        var semicolonToken = GetDefaultToken(TokenType.Semicolon);
        // basic unterminated
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                rightBraceToken
            },
            Array.Empty<Statement>(),
            new ExpressionStatement(new IdentifierExpression("a"))
        };
        // basic terminated
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                rightBraceToken
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                }
            },
            (null as Statement)!
        };
        // terminated and unterminated
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                new(TokenType.Identifier, "b"),
                rightBraceToken
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                }
            },
            new ExpressionStatement(new IdentifierExpression("b"))
        };
        // double terminated
        yield return new object[]
        {
            new[]
            {
                leftBraceToken,
                new(TokenType.Identifier, "a"),
                semicolonToken,
                new(TokenType.Identifier, "b"),
                semicolonToken,
                rightBraceToken
            },
            new Statement[]
            {
                new ExpressionStatement(new IdentifierExpression("a"))
                {
                    IsTerminated = true
                },
                new ExpressionStatement(new IdentifierExpression("b"))
                {
                    IsTerminated = true
                }
            },
            (null as Statement)!
        };
    }

    public static IEnumerable<object[]> GenerateConditionalExpressionTestData()
    {
        var ifToken = GetDefaultToken(TokenType.KeywordIf);
        var leftParenthesisToken = GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = GetDefaultToken(TokenType.RightParenthesis);
        var elifToken = GetDefaultToken(TokenType.KeywordElif);
        var elseToken = GetDefaultToken(TokenType.KeywordElse);
        // basic
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            Array.Empty<ConditionalElement>(),
            (null as Expression)!
        };
        // with else
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                elseToken,
                new(TokenType.Identifier, "c")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            Array.Empty<ConditionalElement>(),
            new IdentifierExpression("c")
        };
        // with elif
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                elifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            new[]
            {
                new ConditionalElement(new IdentifierExpression("c"), new IdentifierExpression("d"))
            },
            (null as Expression)!
        };
        // with more than one elif
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                elifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d"),
                elifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "e"),
                rightParenthesisToken,
                new(TokenType.Identifier, "f")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            new[]
            {
                new ConditionalElement(new IdentifierExpression("c"), new IdentifierExpression("d")),
                new ConditionalElement(new IdentifierExpression("e"), new IdentifierExpression("f"))
            },
            (null as Expression)!
        };
        // with elif and else
        yield return new object[]
        {
            new[]
            {
                ifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b"),
                elifToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d"),
                elseToken,
                new(TokenType.Identifier, "e")
            },
            new ConditionalElement(new IdentifierExpression("a"), new IdentifierExpression("b")),
            new[]
            {
                new ConditionalElement(new IdentifierExpression("c"), new IdentifierExpression("d"))
            },
            new IdentifierExpression("e")
        };
    }

    public static IEnumerable<object[]> GenerateForLoopExpressionTestData()
    {
        var forToken = GetDefaultToken(TokenType.KeywordFor);
        var leftParenthesisToken = GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = GetDefaultToken(TokenType.RightParenthesis);
        var colonToken = GetDefaultToken(TokenType.Colon);
        var comma = GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b")
            },
            null!,
            new ForLoopRange(new IdentifierExpression("a")),
            new IdentifierExpression("b")
        };
        // with start:stop range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                colonToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                new(TokenType.Identifier, "c")
            },
            null!,
            new ForLoopRange(Start: new IdentifierExpression("a"), PastTheEnd: new IdentifierExpression("b")),
            new IdentifierExpression("c")
        };
        // with start:stop:step range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                colonToken,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d")
            },
            null!,
            new ForLoopRange(Start: new IdentifierExpression("a"), PastTheEnd: new IdentifierExpression("b"), Step: new IdentifierExpression("c")),
            new IdentifierExpression("d")
        };
        // with counter
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                comma,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                new(TokenType.Identifier, "c")
            },
            "a",
            new ForLoopRange(new IdentifierExpression("b")),
            new IdentifierExpression("c")
        };
        // with counter and start:stop range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                comma,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken,
                new(TokenType.Identifier, "d")
            },
            "a",
            new ForLoopRange(Start: new IdentifierExpression("b"), PastTheEnd: new IdentifierExpression("c")),
            new IdentifierExpression("d")
        };
        // with counter and start:stop:step range
        yield return new object[]
        {
            new[]
            {
                forToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                comma,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                colonToken,
                new(TokenType.Identifier, "d"),
                rightParenthesisToken,
                new(TokenType.Identifier, "e")
            },
            "a",
            new ForLoopRange(Start: new IdentifierExpression("b"), PastTheEnd: new IdentifierExpression("c"), Step: new IdentifierExpression("d")),
            new IdentifierExpression("e")
        };
    }

    public static IEnumerable<object[]> GenerateWhileLoopExpressionTestData()
    {
        var whileToken = GetDefaultToken(TokenType.KeywordWhile);
        var leftParenthesisToken = GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = GetDefaultToken(TokenType.RightParenthesis);
        // basic
        yield return new object[]
        {
            new[]
            {
                whileToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                new(TokenType.Identifier, "b")
            },
            new IdentifierExpression("a"),
            new IdentifierExpression("b")
        };
    }

    public static IEnumerable<object[]> GenerateFunctionDefinitionExpressionTestData()
    {
        var functiToken = GetDefaultToken(TokenType.KeywordFuncti);
        var leftParenthesisToken = GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = GetDefaultToken(TokenType.RightParenthesis);
        var leftBrace = GetDefaultToken(TokenType.LeftBrace);
        var rightBrace = GetDefaultToken(TokenType.RightBrace);
        var constToken = GetDefaultToken(TokenType.KeywordConst);
        var bangToken = GetDefaultToken(TokenType.OperatorBang);
        var commaToken = GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "a"),
                rightBrace
            },
            Array.Empty<FunctionParameter>(),
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("a")))
        };
        // with one parameter
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter("a")
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))
        };
        // with one const parameter
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                constToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter(IsConst: true, Name: "a")
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))
        };
        // with one required parameter
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                bangToken,
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter("a", IsNullAllowed: false)
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("b")))
        };
        // with more than one parameter
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                commaToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "c"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter("a"),
                new FunctionParameter("b")
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("c")))
        };
        // with more than one parameter (including one const and non-nullable)
        yield return new object[]
        {
            new[]
            {
                functiToken,
                leftParenthesisToken,
                constToken,
                new(TokenType.Identifier, "a"),
                bangToken,
                commaToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "c"),
                rightBrace
            },
            new[]
            {
                new FunctionParameter(IsConst: true, Name: "a", IsNullAllowed: false),
                new FunctionParameter("b")
            },
            new BlockExpression(new List<Statement>(), new ExpressionStatement(new IdentifierExpression("c")))
        };
    }

    public static IEnumerable<object[]> GeneratePatternMatchingExpressionTestData()
    {
        var matchToken = GetDefaultToken(TokenType.KeywordMatch);
        var leftParenthesisToken = GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = GetDefaultToken(TokenType.RightParenthesis);
        var leftBrace = GetDefaultToken(TokenType.LeftBrace);
        var rightBrace = GetDefaultToken(TokenType.RightBrace);
        var colonToken = GetDefaultToken(TokenType.Colon);
        var semicolonToken = GetDefaultToken(TokenType.Semicolon);
        var defaultToken = GetDefaultToken(TokenType.KeywordDefault);
        // basic
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                rightBrace
            },
            new IdentifierExpression("a"),
            Array.Empty<PatternMatchingBranch>(),
            (null as Expression)!
        };
        // with non-default branch
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                semicolonToken,
                rightBrace
            },
            new IdentifierExpression("a"),
            new[]
            {
                new PatternMatchingBranch(new IdentifierExpression("b"), new IdentifierExpression("c"))
            },
            (null as Expression)!
        };
        // with default branch
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                defaultToken,
                colonToken,
                new(TokenType.Identifier, "b"),
                semicolonToken,
                rightBrace
            },
            new IdentifierExpression("a"),
            Array.Empty<PatternMatchingBranch>(),
            new IdentifierExpression("b")
        };
        // with more than one non-default branch
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                semicolonToken,
                new(TokenType.Identifier, "d"),
                colonToken,
                new(TokenType.Identifier, "e"),
                semicolonToken,
                rightBrace
            },
            new IdentifierExpression("a"),
            new[]
            {
                new PatternMatchingBranch(new IdentifierExpression("b"), new IdentifierExpression("c")),
                new PatternMatchingBranch(new IdentifierExpression("d"), new IdentifierExpression("e"))
            },
            (null as Expression)!
        };
        // with both non-default and default branch
        yield return new object[]
        {
            new[]
            {
                matchToken,
                leftParenthesisToken,
                new(TokenType.Identifier, "a"),
                rightParenthesisToken,
                leftBrace,
                new(TokenType.Identifier, "b"),
                colonToken,
                new(TokenType.Identifier, "c"),
                semicolonToken,
                defaultToken,
                colonToken,
                new(TokenType.Identifier, "d"),
                semicolonToken,
                rightBrace
            },
            new IdentifierExpression("a"),
            new[]
            {
                new PatternMatchingBranch(new IdentifierExpression("b"), new IdentifierExpression("c"))
            },
            new IdentifierExpression("d")
        };
    }

    public static IEnumerable<object[]> GenerateFunctionCallExpressionTestData()
    {
        var leftParenthesisToken = GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = GetDefaultToken(TokenType.RightParenthesis);
        var commaToken = GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                leftParenthesisToken,
                rightParenthesisToken
            },
            new IdentifierExpression("a"),
            Array.Empty<Expression>()
        };
        // with an argument
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                leftParenthesisToken,
                new(TokenType.Identifier, "b"),
                rightParenthesisToken
            },
            new IdentifierExpression("a"),
            new[]
            {
                new IdentifierExpression("b")
            }
        };
        // with more than one argument
        yield return new object[]
        {
            new[]
            {
                new(TokenType.Identifier, "a"),
                leftParenthesisToken,
                new(TokenType.Identifier, "b"),
                commaToken,
                new(TokenType.Identifier, "c"),
                rightParenthesisToken
            },
            new IdentifierExpression("a"),
            new[]
            {
                new IdentifierExpression("b"),
                new IdentifierExpression("c")
            }
        };
    }

    public static IEnumerable<object[]> GenerateOperatorsPriorityTestData()
    {
        // same priority () .
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorDot),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.LeftParenthesis),
                GetDefaultToken(TokenType.RightParenthesis),
                GetDefaultToken(TokenType.OperatorDot),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new FunctionCallExpression(new BinaryExpression(
                    new IdentifierExpression("a"),
                    Operator.NamespaceAccess,
                    new IdentifierExpression("b")), new List<Expression>()),
                Operator.NamespaceAccess,
                new IdentifierExpression("c"))
        };
        // . higher than ^
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorCaret),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorDot),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.Exponentiation,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.NamespaceAccess,
                    new IdentifierExpression("c")))
        };
        // ^ higher than unary +
        yield return new object[]
        {
            new[]
            {
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorCaret),
                new Token(TokenType.Identifier, "b")
            },
            new UnaryExpression(
                Operator.NumberPromotion,
                new BinaryExpression(
                    new IdentifierExpression("a"),
                    Operator.Exponentiation,
                    new IdentifierExpression("b")))
        };
        // same priority unary + unary - unary !
        yield return new object[]
        {
            new[]
            {
                GetDefaultToken(TokenType.OperatorPlus),
                GetDefaultToken(TokenType.OperatorMinus),
                GetDefaultToken(TokenType.OperatorBang),
                GetDefaultToken(TokenType.OperatorMinus),
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a")
            },
            new UnaryExpression(
                Operator.NumberPromotion,
                new UnaryExpression(
                    Operator.ArithmeticNegation,
                    new UnaryExpression(
                        Operator.LogicalNegation,
                        new UnaryExpression(
                            Operator.ArithmeticNegation,
                            new UnaryExpression(
                                Operator.NumberPromotion,
                                new IdentifierExpression("a"))))))
        };
        // unary + higher than *
        yield return new object[]
        {
            new[]
            {
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorAsterisk),
                new Token(TokenType.Identifier, "b")
            },
            new BinaryExpression(
                new UnaryExpression(
                    Operator.NumberPromotion,
                    new IdentifierExpression("a")),
                Operator.Multiplication,
                new IdentifierExpression("b"))
        };
        // same priority * / %
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorAsterisk),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorSlash),
                new Token(TokenType.Identifier, "c"),
                GetDefaultToken(TokenType.OperatorPercent),
                new Token(TokenType.Identifier, "d"),
                GetDefaultToken(TokenType.OperatorSlash),
                new Token(TokenType.Identifier, "e"),
                GetDefaultToken(TokenType.OperatorAsterisk),
                new Token(TokenType.Identifier, "f")
            },
            new BinaryExpression(
                new BinaryExpression(
                    new BinaryExpression(
                        new BinaryExpression(
                            new BinaryExpression(
                                new IdentifierExpression("a"),
                                Operator.Multiplication,
                                new IdentifierExpression("b")),
                            Operator.Division,
                            new IdentifierExpression("c")),
                        Operator.Remainder,
                        new IdentifierExpression("d")),
                    Operator.Division,
                    new IdentifierExpression("e")),
                Operator.Multiplication,
                new IdentifierExpression("f"))
        };
        // * higher than binary +
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorAsterisk),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.Addition,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.Multiplication,
                    new IdentifierExpression("c")))
        };
        // same priority binary + binary -
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorMinus),
                new Token(TokenType.Identifier, "c"),
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "d")
            },
            new BinaryExpression(
                new BinaryExpression(
                    new BinaryExpression(
                        new IdentifierExpression("a"),
                        Operator.Addition,
                        new IdentifierExpression("b")),
                    Operator.Subtraction,
                    new IdentifierExpression("c")),
                Operator.Addition,
                new IdentifierExpression("d"))
        };
        // binary + higher than ..
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorDotDot),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.Concatenation,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.Addition,
                    new IdentifierExpression("c")))
        };
        // .. higher than <
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorDotDot),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.LessThanComparison,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.Concatenation,
                    new IdentifierExpression("c")))
        };
        // same priority < <= > >= == !=
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorLessEquals),
                new Token(TokenType.Identifier, "c"),
                GetDefaultToken(TokenType.OperatorGreater),
                new Token(TokenType.Identifier, "d"),
                GetDefaultToken(TokenType.OperatorGreaterEquals),
                new Token(TokenType.Identifier, "e"),
                GetDefaultToken(TokenType.OperatorEqualsEquals),
                new Token(TokenType.Identifier, "f"),
                GetDefaultToken(TokenType.OperatorBangEquals),
                new Token(TokenType.Identifier, "g"),
                GetDefaultToken(TokenType.OperatorEqualsEquals),
                new Token(TokenType.Identifier, "h"),
                GetDefaultToken(TokenType.OperatorGreaterEquals),
                new Token(TokenType.Identifier, "i"),
                GetDefaultToken(TokenType.OperatorGreater),
                new Token(TokenType.Identifier, "j"),
                GetDefaultToken(TokenType.OperatorLessEquals),
                new Token(TokenType.Identifier, "k"),
                GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "l")
            },
            new BinaryExpression(
                new BinaryExpression(
                    new BinaryExpression(
                        new BinaryExpression(
                            new BinaryExpression(
                                new BinaryExpression(
                                    new BinaryExpression(
                                        new BinaryExpression(
                                            new BinaryExpression(
                                                new BinaryExpression(
                                                    new BinaryExpression(
                                                        new IdentifierExpression("a"),
                                                        Operator.LessThanComparison,
                                                        new IdentifierExpression("b")),
                                                    Operator.LessOrEqualComparison,
                                                    new IdentifierExpression("c")),
                                                Operator.GreaterThanComparison,
                                                new IdentifierExpression("d")),
                                            Operator.GreaterOrEqualComparison,
                                            new IdentifierExpression("e")),
                                        Operator.EqualComparison,
                                        new IdentifierExpression("f")),
                                    Operator.NotEqualComparison,
                                    new IdentifierExpression("g")),
                                Operator.EqualComparison,
                                new IdentifierExpression("h")),
                            Operator.GreaterOrEqualComparison,
                            new IdentifierExpression("i")),
                        Operator.GreaterThanComparison,
                        new IdentifierExpression("j")),
                    Operator.LessOrEqualComparison,
                    new IdentifierExpression("k")),
                Operator.LessThanComparison,
                new IdentifierExpression("l"))
        };
        // < higher than is
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.KeywordIs),
                GetDefaultToken(TokenType.KeywordInt)
            },
            new BinaryExpression(
                new BinaryExpression(
                    new IdentifierExpression("a"),
                    Operator.LessThanComparison,
                    new IdentifierExpression("b")
                ),
                Operator.EqualTypeCheck,
                new TypeExpression(DataType.Integer))
        };
        // < higher than is not
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.KeywordIs),
                GetDefaultToken(TokenType.KeywordNot),
                GetDefaultToken(TokenType.KeywordInt)
            },
            new BinaryExpression(
                new BinaryExpression(
                    new IdentifierExpression("a"),
                    Operator.LessThanComparison,
                    new IdentifierExpression("b")
                ),
                Operator.NotEqualTypeCheck,
                new TypeExpression(DataType.Integer))
        };
        // is higher than &&
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorAndAnd),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.KeywordIs),
                GetDefaultToken(TokenType.KeywordInt)
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.Conjunction,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.EqualTypeCheck,
                    new TypeExpression(DataType.Integer)))
        };
        // is not higher than &&
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorAndAnd),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.KeywordIs),
                GetDefaultToken(TokenType.KeywordNot),
                GetDefaultToken(TokenType.KeywordInt)
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.Conjunction,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.NotEqualTypeCheck,
                    new TypeExpression(DataType.Integer)))
        };
        // && higher than ||
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorOrOr),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorAndAnd),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.Disjunction,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.Conjunction,
                    new IdentifierExpression("c")))
        };
        // || higher than ?>
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorQueryGreater),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorOrOr),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.NullSafePipe,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.Disjunction,
                    new IdentifierExpression("c")))
        };
        // ?> higher than ??
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorQueryQuery),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorQueryGreater),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.NullCoalescing,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.NullSafePipe,
                    new IdentifierExpression("c")))
        };
        // ?? higher than =
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorEquals),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorQueryQuery),
                new Token(TokenType.Identifier, "c")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.Assignment,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.NullCoalescing,
                    new IdentifierExpression("c")))
        };
        // same priority = += -= *= /= %=
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorEquals),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.OperatorPlusEquals),
                new Token(TokenType.Identifier, "c"),
                GetDefaultToken(TokenType.OperatorMinusEquals),
                new Token(TokenType.Identifier, "d"),
                GetDefaultToken(TokenType.OperatorAsteriskEquals),
                new Token(TokenType.Identifier, "e"),
                GetDefaultToken(TokenType.OperatorSlashEquals),
                new Token(TokenType.Identifier, "f"),
                GetDefaultToken(TokenType.OperatorPercentEquals),
                new Token(TokenType.Identifier, "g"),
                GetDefaultToken(TokenType.OperatorSlashEquals),
                new Token(TokenType.Identifier, "h"),
                GetDefaultToken(TokenType.OperatorAsteriskEquals),
                new Token(TokenType.Identifier, "i"),
                GetDefaultToken(TokenType.OperatorMinusEquals),
                new Token(TokenType.Identifier, "j"),
                GetDefaultToken(TokenType.OperatorPlusEquals),
                new Token(TokenType.Identifier, "k"),
                GetDefaultToken(TokenType.OperatorEquals),
                new Token(TokenType.Identifier, "l")
            },
            new BinaryExpression(
                new IdentifierExpression("a"),
                Operator.Assignment,
                new BinaryExpression(
                    new IdentifierExpression("b"),
                    Operator.AdditionAssignment,
                    new BinaryExpression(
                        new IdentifierExpression("c"),
                        Operator.SubtractionAssignment,
                        new BinaryExpression(
                            new IdentifierExpression("d"),
                            Operator.MultiplicationAssignment,
                            new BinaryExpression(
                                new IdentifierExpression("e"),
                                Operator.DivisionAssignment,
                                new BinaryExpression(
                                    new IdentifierExpression("f"),
                                    Operator.RemainderAssignment,
                                    new BinaryExpression(
                                        new IdentifierExpression("g"),
                                        Operator.DivisionAssignment,
                                        new BinaryExpression(
                                            new IdentifierExpression("h"),
                                            Operator.MultiplicationAssignment,
                                            new BinaryExpression(
                                                new IdentifierExpression("i"),
                                                Operator.SubtractionAssignment,
                                                new BinaryExpression(
                                                    new IdentifierExpression("j"),
                                                    Operator.AdditionAssignment,
                                                    new BinaryExpression(
                                                        new IdentifierExpression("k"),
                                                        Operator.Assignment,
                                                        new IdentifierExpression("l"))))))))))))
        };
    }

    public static IEnumerable<object[]> GenerateOperatorsAssociativityTestData()
    {
        static object[] GenerateLeftBinary(TokenType tokenType, Operator @operator) =>
            new object[]
            {
                new[]
                {
                    new Token(TokenType.Identifier, "a"),
                    GetDefaultToken(tokenType),
                    new Token(TokenType.Identifier, "b"),
                    GetDefaultToken(tokenType),
                    new Token(TokenType.Identifier, "c")
                },
                new BinaryExpression(
                    new BinaryExpression(
                        new IdentifierExpression("a"),
                        @operator,
                        new IdentifierExpression("b")),
                    @operator,
                    new IdentifierExpression("c"))
            };
        static object[] GenerateRightBinary(TokenType tokenType, Operator @operator) =>
            new object[]
            {
                new[]
                {
                    new Token(TokenType.Identifier, "a"),
                    GetDefaultToken(tokenType),
                    new Token(TokenType.Identifier, "b"),
                    GetDefaultToken(tokenType),
                    new Token(TokenType.Identifier, "c")
                },
                new BinaryExpression(
                    new IdentifierExpression("a"),
                    @operator,
                    new BinaryExpression(
                        new IdentifierExpression("b"),
                        @operator,
                        new IdentifierExpression("c")))
            };
        static object[] GenerateRightUnary(TokenType tokenType, Operator @operator) =>
            new object[]
            {
                new[]
                {
                    GetDefaultToken(tokenType),
                    GetDefaultToken(tokenType),
                    new Token(TokenType.Identifier, "a")
                },
                new UnaryExpression(
                    @operator,
                    new UnaryExpression(
                        @operator,
                        new IdentifierExpression("a")))
            };
        static object[] GenerateTypeCheck(bool isNegated = false) =>
            new object[]
            {
                new[]
                {
                    new Token(TokenType.Identifier, "a"),
                    GetDefaultToken(TokenType.KeywordIs),
                    GetDefaultToken(TokenType.KeywordInt),
                    GetDefaultToken(TokenType.KeywordIs),
                    GetDefaultToken(TokenType.KeywordInt)
                }.SelectMany(x =>
                    x.Type == TokenType.KeywordIs
                        ? isNegated ? new[] { x, GetDefaultToken(TokenType.KeywordNot) } : new[] { x }
                        : new[] { x }).ToArray(),
                new BinaryExpression(
                    new BinaryExpression(
                        new IdentifierExpression("a"),
                        isNegated ? Operator.NotEqualTypeCheck : Operator.EqualTypeCheck,
                        new TypeExpression(DataType.Integer)),
                    isNegated ? Operator.NotEqualTypeCheck : Operator.EqualTypeCheck,
                    new TypeExpression(DataType.Integer))
            };

        // .
        yield return GenerateLeftBinary(TokenType.OperatorDot, Operator.NamespaceAccess);
        // ()
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.LeftParenthesis),
                new Token(TokenType.Identifier, "b"),
                GetDefaultToken(TokenType.RightParenthesis),
                GetDefaultToken(TokenType.LeftParenthesis),
                new Token(TokenType.Identifier, "c"),
                GetDefaultToken(TokenType.RightParenthesis)
            },
            new FunctionCallExpression(
                new FunctionCallExpression(
                    new IdentifierExpression("a"),
                    new List<Expression> { new IdentifierExpression("b") }),
                new List<Expression> { new IdentifierExpression("c") })
        };
        // ^
        yield return GenerateRightBinary(TokenType.OperatorCaret, Operator.Exponentiation);
        // unary +
        yield return GenerateRightUnary(TokenType.OperatorPlus, Operator.NumberPromotion);
        // unary -
        yield return GenerateRightUnary(TokenType.OperatorMinus, Operator.ArithmeticNegation);
        // unary !
        yield return GenerateRightUnary(TokenType.OperatorBang, Operator.LogicalNegation);
        // *
        yield return GenerateLeftBinary(TokenType.OperatorAsterisk, Operator.Multiplication);
        // /
        yield return GenerateLeftBinary(TokenType.OperatorSlash, Operator.Division);
        // %
        yield return GenerateLeftBinary(TokenType.OperatorPercent, Operator.Remainder);
        // binary +
        yield return GenerateLeftBinary(TokenType.OperatorPlus, Operator.Addition);
        // binary -
        yield return GenerateLeftBinary(TokenType.OperatorMinus, Operator.Subtraction);
        // ..
        yield return GenerateLeftBinary(TokenType.OperatorDotDot, Operator.Concatenation);
        // <
        yield return GenerateLeftBinary(TokenType.OperatorLess, Operator.LessThanComparison);
        // <=
        yield return GenerateLeftBinary(TokenType.OperatorLessEquals, Operator.LessOrEqualComparison);
        // >
        yield return GenerateLeftBinary(TokenType.OperatorGreater, Operator.GreaterThanComparison);
        // >=
        yield return GenerateLeftBinary(TokenType.OperatorGreaterEquals, Operator.GreaterOrEqualComparison);
        // ==
        yield return GenerateLeftBinary(TokenType.OperatorEqualsEquals, Operator.EqualComparison);
        // !=
        yield return GenerateLeftBinary(TokenType.OperatorBangEquals, Operator.NotEqualComparison);
        // is
        yield return GenerateTypeCheck();
        // is not
        yield return GenerateTypeCheck(true);
        // &&
        yield return GenerateLeftBinary(TokenType.OperatorAndAnd, Operator.Conjunction);
        // ||
        yield return GenerateLeftBinary(TokenType.OperatorOrOr, Operator.Disjunction);
        // ?>
        yield return GenerateLeftBinary(TokenType.OperatorQueryGreater, Operator.NullSafePipe);
        // ??
        yield return GenerateLeftBinary(TokenType.OperatorQueryQuery, Operator.NullCoalescing);
        // =
        yield return GenerateRightBinary(TokenType.OperatorEquals, Operator.Assignment);
        // +=
        yield return GenerateRightBinary(TokenType.OperatorPlusEquals, Operator.AdditionAssignment);
        // -=
        yield return GenerateRightBinary(TokenType.OperatorMinusEquals, Operator.SubtractionAssignment);
        // *=
        yield return GenerateRightBinary(TokenType.OperatorAsteriskEquals, Operator.MultiplicationAssignment);
        // /=
        yield return GenerateRightBinary(TokenType.OperatorSlashEquals, Operator.DivisionAssignment);
        // %=
        yield return GenerateRightBinary(TokenType.OperatorPercentEquals, Operator.RemainderAssignment);
    }

    #endregion Generators
}
