using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class ExpressionStatementTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // block
        yield return new[]
        {
            new BlockExpressionTestData().First()[0],
            typeof(BlockExpression)
        };
        // if
        yield return new[]
        {
            new ConditionalExpressionTestData().First()[0],
            typeof(ConditionalExpression)
        };
        // for
        yield return new[]
        {
            new ForLoopExpressionTestData().First()[0],
            typeof(ForLoopExpression)
        };
        // while
        yield return new[]
        {
            new WhileLoopExpressionTestData().First()[0],
            typeof(WhileLoopExpression)
        };
        // functi
        yield return new[]
        {
            new FunctionDefinitionExpressionTestData().First()[0],
            typeof(FunctionDefinitionExpression)
        };
        // match
        yield return new[]
        {
            new PatternMatchingExpressionTestData().First()[0],
            typeof(PatternMatchingExpression)
        };
        // binary
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "b")
            },
            typeof(BinaryExpression)
        };
        // unary
        yield return new object[]
        {
            new[]
            {
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a")
            },
            typeof(UnaryExpression)
        };
        // call
        yield return new[]
        {
            new FunctionCallExpressionTestData().First()[0],
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
        // type cast
        yield return new object[]
        {
            new[]
            {
                Helpers.GetDefaultToken(TokenType.KeywordInt),
                Helpers.GetDefaultToken(TokenType.LeftParenthesis),
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.RightParenthesis)
            },
            typeof(TypeCastExpression)
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
