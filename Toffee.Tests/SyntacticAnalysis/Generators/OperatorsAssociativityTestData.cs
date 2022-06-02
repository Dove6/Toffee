using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class OperatorsAssociativityTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        static object[] GenerateLeftBinary(TokenType tokenType, Operator @operator) =>
            new object[]
            {
                new[]
                {
                    new Token(TokenType.Identifier, "a"),
                    Helpers.GetDefaultToken(tokenType),
                    new Token(TokenType.Identifier, "b"),
                    Helpers.GetDefaultToken(tokenType),
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
                    Helpers.GetDefaultToken(tokenType),
                    new Token(TokenType.Identifier, "b"),
                    Helpers.GetDefaultToken(tokenType),
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
                    Helpers.GetDefaultToken(tokenType),
                    Helpers.GetDefaultToken(tokenType),
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
                    Helpers.GetDefaultToken(TokenType.KeywordIs),
                    Helpers.GetDefaultToken(TokenType.KeywordInt),
                    Helpers.GetDefaultToken(TokenType.KeywordIs),
                    Helpers.GetDefaultToken(TokenType.KeywordInt)
                }.SelectMany(x =>
                    x.Type == TokenType.KeywordIs
                        ? isNegated ? new[] { x, Helpers.GetDefaultToken(TokenType.KeywordNot) } : new[] { x }
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
                Helpers.GetDefaultToken(TokenType.LeftParenthesis),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.RightParenthesis),
                Helpers.GetDefaultToken(TokenType.LeftParenthesis),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.RightParenthesis)
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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
