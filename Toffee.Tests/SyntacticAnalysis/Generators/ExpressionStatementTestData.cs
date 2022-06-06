using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;
using Toffee.Tests.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class ExpressionStatementTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // block
        yield return new object[]
        {
            (Token[])new BlockExpressionTestData().First()[0],
            typeof(BlockExpression)
        };
        // if
        yield return new object[]
        {
            (Token[])new ConditionalExpressionTestData().First()[0],
            typeof(ConditionalExpression)
        };
        // for
        yield return new object[]
        {
            (Token[])new ForLoopExpressionTestData().First()[0],
            typeof(ForLoopExpression)
        };
        // while
        yield return new object[]
        {
            (Token[])new WhileLoopExpressionTestData().First()[0],
            typeof(WhileLoopExpression)
        };
        // functi
        yield return new object[]
        {
            (Token[])new FunctionDefinitionExpressionTestData().First()[0],
            typeof(FunctionDefinitionExpression)
        };
        // match
        yield return new object[]
        {
            (Token[])new PatternMatchingExpressionTestData().First()[0],
            typeof(BlockExpression),
            true
        };
        // binary
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
            },
            typeof(BinaryExpression)
        };
        // unary
        yield return new object[]
        {
            new[]
            {
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
            },
            typeof(UnaryExpression)
        };
        // call
        yield return new object[]
        {
            (Token[])new FunctionCallExpressionTestData().First()[0],
            typeof(FunctionCallExpression)
        };
        // identifier
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
            },
            typeof(IdentifierExpression)
        };
        // literal
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.LiteralInteger, 18ul),
                Helpers.GetDefaultToken(TokenType.Semicolon)
            },
            typeof(LiteralExpression)
        };
        // type cast
        yield return new object[]
        {
            new[]
            {
                Helpers.GetDefaultToken(TokenType.KeywordInt),
                Helpers.GetDefaultToken(TokenType.LeftParenthesis),
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.RightParenthesis),
                Helpers.GetDefaultToken(TokenType.Semicolon)
            },
            typeof(TypeCastExpression)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
