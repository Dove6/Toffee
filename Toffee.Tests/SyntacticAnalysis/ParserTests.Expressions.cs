﻿using System;
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
}
