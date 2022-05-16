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
    [Trait("Category", "Variable initialization list statements")]
    [Theory]
    [InlineData("std")]
    [InlineData("std", "io")]
    [InlineData("one1", "two2", "three3")]
    public void NamespaceImportStatementsShouldBeParsedCorrectly(params string[] namespaceSegments)
    {
        var pullToken = GetDefaultToken(TokenType.KeywordPull);

        var namespaceSegmentTokens = namespaceSegments.Select(x => new Token(TokenType.Identifier, x));
        var dotToken =GetDefaultToken(TokenType.OperatorDot);
        var interleavedNamespaceSegments = namespaceSegmentTokens.SelectMany(x => new[] { x, dotToken })
            .Take(2 * namespaceSegments.Length - 1);

        var expectedNamespaceSegments = namespaceSegments.Select(x => new IdentifierExpression(x)).ToArray();

        var lexerMock = new LexerMock(interleavedNamespaceSegments.Prepend(pullToken).ToArray());

        IParser parser = new Parser(lexerMock);

        var namespaceImportStatement = parser.CurrentStatement.As<NamespaceImportStatement>();
        namespaceImportStatement.Should().NotBeNull();
        namespaceImportStatement!.IsTerminated.Should().Be(false);
        namespaceImportStatement.NamespaceLevels.ToArray().Should().BeEquivalentTo(expectedNamespaceSegments, ProvideOptions);
    }

    [Trait("Category", "Variable initialization list statements")]
    [Theory]
    [MemberData(nameof(GenerateVariableInitializationListStatementTestData))]
    public void VariableInitializationListStatementsShouldBeParsedCorrectly(Token[] tokenSequence, VariableInitialization[] expectedVariableList)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var returnStatement = parser.CurrentStatement.As<VariableInitializationListStatement>();
        returnStatement.Should().NotBeNull();
        returnStatement!.IsTerminated.Should().Be(false);
        returnStatement.Items.Should().BeEquivalentTo(expectedVariableList, ProvideOptions);
    }

    [Trait("Category", "Return statements")]
    [Fact]
    public void EmptyReturnStatementsShouldBeParsedCorrectly()
    {
        var returnToken = GetDefaultToken(TokenType.KeywordReturn);

        var lexerMock = new LexerMock(returnToken);

        IParser parser = new Parser(lexerMock);

        var returnStatement = parser.CurrentStatement.As<ReturnStatement>();
        returnStatement.Should().NotBeNull();
        returnStatement!.IsTerminated.Should().Be(false);
        returnStatement.Value.Should().BeNull();
    }

    [Trait("Category", "Return statements")]
    [Fact]
    public void ReturnStatementsContainingExpressionsShouldBeParsedCorrectly()
    {
        var returnToken = GetDefaultToken(TokenType.KeywordReturn);

        const string identifierName = "a";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var lexerMock = new LexerMock(returnToken, identifierToken);

        IParser parser = new Parser(lexerMock);

        var returnStatement = parser.CurrentStatement.As<ReturnStatement>();
        returnStatement.Should().NotBeNull();
        returnStatement!.IsTerminated.Should().Be(false);
        returnStatement.Value.Should().NotBeNull();

        var expression = returnStatement.Value.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression.Name.Should().Be(identifierName);
    }

    [Trait("Category", "Break statements")]
    [Fact]
    public void BreakStatementsShouldBeParsedCorrectly()
    {
        var breakToken = GetDefaultToken(TokenType.KeywordBreak);

        var lexerMock = new LexerMock(breakToken);

        IParser parser = new Parser(lexerMock);

        var breakStatement = parser.CurrentStatement.As<BreakStatement>();
        breakStatement.Should().NotBeNull();
        breakStatement!.IsTerminated.Should().Be(false);
    }

    [Trait("Category", "Break if statements")]
    [Fact]
    public void BreakIfStatementsShouldBeParsedCorrectly()
    {
        var breakIfToken = GetDefaultToken(TokenType.KeywordBreakIf);

        var leftParenthesisToken = GetDefaultToken(TokenType.LeftParenthesis);

        const string identifierName = "a";
        var identifierToken = new Token(TokenType.Identifier, identifierName);

        var rightParenthesisToken = GetDefaultToken(TokenType.RightParenthesis);

        var lexerMock = new LexerMock(breakIfToken, leftParenthesisToken, identifierToken, rightParenthesisToken);

        IParser parser = new Parser(lexerMock);

        var breakIfStatement = parser.CurrentStatement.As<BreakIfStatement>();
        breakIfStatement.Should().NotBeNull();
        breakIfStatement!.IsTerminated.Should().Be(false);

        var expression = breakIfStatement.Condition.As<IdentifierExpression>();
        expression.Should().NotBeNull();
        expression!.Name.Should().Be(identifierName);
    }

    [Trait("Category", "Expression statements")]
    [Theory]
    [MemberData(nameof(GenerateExpressionStatementTestData))]
    public void ExpressionStatementsShouldBeParsedCorrectly(Token[] tokenSequence, Type expectedExpressionType)
    {
        var lexerMock = new LexerMock(tokenSequence);

        IParser parser = new Parser(lexerMock);

        var expressionStatement = parser.CurrentStatement.As<ExpressionStatement>();
        expressionStatement.Should().NotBeNull();
        expressionStatement!.IsTerminated.Should().Be(false);
        expressionStatement.Expression.Should().BeOfType(expectedExpressionType);
    }

    [Trait("Category", "Comments")]
    [Fact]
    public void CommentsShouldBeIgnoredWhileParsing()
    {
        var initToken = GetDefaultToken(TokenType.KeywordInit);
        var constToken = GetDefaultToken(TokenType.KeywordConst);
        var identifierToken = new Token(TokenType.Identifier, "a");
        var equalToken = GetDefaultToken(TokenType.OperatorEquals);
        var leftTermToken = new Token(TokenType.LiteralInteger, 123L);
        var plusToken = GetDefaultToken(TokenType.OperatorPlus);
        var rightTermToken = new Token(TokenType.LiteralFloat, 3.14);

        var expectedStatement = new VariableInitializationListStatement(new List<VariableInitialization>
        {
            new("a",
                new BinaryExpression(new LiteralExpression(DataType.Integer, 123L),
                    Operator.Addition,
                        new LiteralExpression(DataType.Float, 3.14)),
                    true)
        });

        var comments = new[]
        {
            new Token(TokenType.LineComment, "line comment"),
            new Token(TokenType.BlockComment, "block comment")
        };

        var lexerMock =
            new LexerMock(
                new[] { initToken, constToken, identifierToken, equalToken, leftTermToken, plusToken, rightTermToken }
                    .SelectMany((x, i) => new[] { x, comments[i % 2] }).ToArray());

        IParser parser = new Parser(lexerMock);

        var statement = parser.CurrentStatement.As<VariableInitializationListStatement>();
        statement.Should().NotBeNull();
        statement.Should().BeEquivalentTo(expectedStatement, ProvideOptions);
    }

    #region Generators

    public static IEnumerable<object[]> GenerateVariableInitializationListStatementTestData()
    {
        var initToken = GetDefaultToken(TokenType.KeywordInit);
        var constToken = GetDefaultToken(TokenType.KeywordConst);
        var assignmentToken = GetDefaultToken(TokenType.OperatorEquals);
        var commaToken = GetDefaultToken(TokenType.Comma);
        // basic
        yield return new object[]
        {
            new[]
            {
                initToken,
                new(TokenType.Identifier, "a")
            },
            new VariableInitialization[]
            {
                new("a")
            }
        };
        // with const
        yield return new object[]
        {
            new[]
            {
                initToken,
                constToken,
                new(TokenType.Identifier, "a")
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a")
            }
        };
        // with initialization
        yield return new object[]
        {
            new[]
            {
                initToken,
                new(TokenType.Identifier, "a"),
                assignmentToken,
                new Token(TokenType.LiteralInteger, 5L)
            },
            new VariableInitialization[]
            {
                new("a", new LiteralExpression(DataType.Integer, 5L))
            }
        };
        // with const and initialization
        yield return new object[]
        {
            new[]
            {
                initToken,
                constToken,
                new(TokenType.Identifier, "a"),
                assignmentToken,
                new Token(TokenType.LiteralInteger, 5L)
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a", InitialValue: new LiteralExpression(DataType.Integer, 5L))
            }
        };
        // more than one
        yield return new object[]
        {
            new[]
            {
                initToken,
                new(TokenType.Identifier, "a"),
                commaToken,
                new(TokenType.Identifier, "b")
            },
            new VariableInitialization[]
            {
                new("a"),
                new("b")
            }
        };
        // more than one, with const and initialization
        yield return new object[]
        {
            new[]
            {
                initToken,
                constToken,
                new(TokenType.Identifier, "a"),
                assignmentToken,
                new Token(TokenType.LiteralInteger, 5L),
                commaToken,
                new(TokenType.Identifier, "b")
            },
            new VariableInitialization[]
            {
                new(IsConst: true, Name: "a", InitialValue: new LiteralExpression(DataType.Integer, 5L)),
                new("b")
            }
        };
    }

    public static IEnumerable<object[]> GenerateExpressionStatementTestData()
    {
        // block
        yield return new[]
        {
            GenerateBlockExpressionTestData().First()[0],
            typeof(BlockExpression)
        };
        // if
        yield return new[]
        {
            GenerateConditionalExpressionTestData().First()[0],
            typeof(ConditionalExpression)
        };
        // for
        yield return new[]
        {
            GenerateForLoopExpressionTestData().First()[0],
            typeof(ForLoopExpression)
        };
        // while
        yield return new[]
        {
            GenerateWhileLoopExpressionTestData().First()[0],
            typeof(WhileLoopExpression)
        };
        // functi
        yield return new[]
        {
            GenerateFunctionDefinitionExpressionTestData().First()[0],
            typeof(FunctionDefinitionExpression)
        };
        // match
        yield return new[]
        {
            GeneratePatternMatchingExpressionTestData().First()[0],
            typeof(PatternMatchingExpression)
        };
        // binary
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "b")
            },
            typeof(BinaryExpression)
        };
        // unary
        yield return new object[]
        {
            new[]
            {
                GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a")
            },
            typeof(UnaryExpression)
        };
        // call
        yield return new[]
        {
            GenerateFunctionCallExpressionTestData().First()[0],
            typeof(FunctionCallExpression)
        };
        // identifier
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a")
            },
            typeof(IdentifierExpression)
        };
        // literal
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.LiteralInteger, 18L)
            },
            typeof(LiteralExpression)
        };
        // TODO: type cast
        // type
        // yield return new object[]
        // {
        //     new[]
        //     {
        //         GetDefaultToken(TokenType.KeywordInt)
        //     },
        //     typeof(TypeExpression)
        // };
    }

    #endregion Generators
}
