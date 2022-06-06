using System;
using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class PatternMatchingExpressionTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var matchToken = Helpers.GetDefaultToken(TokenType.KeywordMatch);
        var leftParenthesisToken = Helpers.GetDefaultToken(TokenType.LeftParenthesis);
        var rightParenthesisToken = Helpers.GetDefaultToken(TokenType.RightParenthesis);
        var leftBrace = Helpers.GetDefaultToken(TokenType.LeftBrace);
        var rightBrace = Helpers.GetDefaultToken(TokenType.RightBrace);
        var colonToken = Helpers.GetDefaultToken(TokenType.Colon);
        var semicolonToken = Helpers.GetDefaultToken(TokenType.Semicolon);
        var defaultToken = Helpers.GetDefaultToken(TokenType.KeywordDefault);
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
                rightBrace,
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>()),
            typeof(DefaultBranchMissing)
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
                rightBrace,
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
            {
                new(new FunctionCallExpression(new IdentifierExpression("b"),
                        new List<Expression> { new IdentifierExpression("a") }),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("c")))
            }),
            typeof(DefaultBranchMissing)
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
                rightBrace,
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>(),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("b")))
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
                rightBrace,
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
            {
                new(new FunctionCallExpression(new IdentifierExpression("b"),
                        new List<Expression> { new IdentifierExpression("a") }),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("c"))),
                new(new FunctionCallExpression(new IdentifierExpression("d"),
                        new List<Expression> { new IdentifierExpression("a") }),
                    new BlockExpression(new List<Statement>(), new IdentifierExpression("e")))
            }),
            typeof(DefaultBranchMissing)
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
                rightBrace,
                semicolonToken
            },
            new ConditionalExpression(new List<ConditionalElement>
                {
                    new(new FunctionCallExpression(new IdentifierExpression("b"),
                            new List<Expression> { new IdentifierExpression("a") }),
                        new BlockExpression(new List<Statement>(), new IdentifierExpression("c")))
                },
                new BlockExpression(new List<Statement>(), new IdentifierExpression("d")))
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
