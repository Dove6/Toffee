using System.Collections;
using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.SyntacticAnalysis;

namespace Toffee.Tests.SyntacticAnalysis.Generators;

public class OperatorsPriorityTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // same priority () .
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.OperatorDot),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.LeftParenthesis),
                Helpers.GetDefaultToken(TokenType.RightParenthesis),
                Helpers.GetDefaultToken(TokenType.OperatorDot),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
            },
            new BinaryExpression(
                new FunctionCallExpression(new BinaryExpression(
                    new IdentifierExpression("a"),
                    Operator.NamespaceAccess,
                    new IdentifierExpression("b")), new List<Expression>()),
                Operator.NamespaceAccess,
                new IdentifierExpression("c")),
            true  // ignore errors (function call cannot be a part of namespace access)
        };
        // . higher than ^
        yield return new object[]
        {
            new[]
            {
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.OperatorCaret),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorDot),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.OperatorCaret),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                Helpers.GetDefaultToken(TokenType.OperatorMinus),
                Helpers.GetDefaultToken(TokenType.OperatorBang),
                Helpers.GetDefaultToken(TokenType.OperatorMinus),
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "a"),
                Helpers.GetDefaultToken(TokenType.OperatorAsterisk),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorAsterisk),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorSlash),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.OperatorPercent),
                new Token(TokenType.Identifier, "d"),
                Helpers.GetDefaultToken(TokenType.OperatorSlash),
                new Token(TokenType.Identifier, "e"),
                Helpers.GetDefaultToken(TokenType.OperatorAsterisk),
                new Token(TokenType.Identifier, "f"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorAsterisk),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorMinus),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "d"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorDotDot),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorPlus),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorDotDot),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorLessEquals),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.OperatorGreater),
                new Token(TokenType.Identifier, "d"),
                Helpers.GetDefaultToken(TokenType.OperatorGreaterEquals),
                new Token(TokenType.Identifier, "e"),
                Helpers.GetDefaultToken(TokenType.OperatorEqualsEquals),
                new Token(TokenType.Identifier, "f"),
                Helpers.GetDefaultToken(TokenType.OperatorBangEquals),
                new Token(TokenType.Identifier, "g"),
                Helpers.GetDefaultToken(TokenType.OperatorEqualsEquals),
                new Token(TokenType.Identifier, "h"),
                Helpers.GetDefaultToken(TokenType.OperatorGreaterEquals),
                new Token(TokenType.Identifier, "i"),
                Helpers.GetDefaultToken(TokenType.OperatorGreater),
                new Token(TokenType.Identifier, "j"),
                Helpers.GetDefaultToken(TokenType.OperatorLessEquals),
                new Token(TokenType.Identifier, "k"),
                Helpers.GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "l"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.KeywordIs),
                Helpers.GetDefaultToken(TokenType.KeywordInt),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorLess),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.KeywordIs),
                Helpers.GetDefaultToken(TokenType.KeywordNot),
                Helpers.GetDefaultToken(TokenType.KeywordInt),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorAndAnd),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.KeywordIs),
                Helpers.GetDefaultToken(TokenType.KeywordInt),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorAndAnd),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.KeywordIs),
                Helpers.GetDefaultToken(TokenType.KeywordNot),
                Helpers.GetDefaultToken(TokenType.KeywordInt),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorOrOr),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorAndAnd),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorQueryGreater),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorOrOr),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorQueryQuery),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorQueryGreater),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorEquals),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorQueryQuery),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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
                Helpers.GetDefaultToken(TokenType.OperatorEquals),
                new Token(TokenType.Identifier, "b"),
                Helpers.GetDefaultToken(TokenType.OperatorPlusEquals),
                new Token(TokenType.Identifier, "c"),
                Helpers.GetDefaultToken(TokenType.OperatorMinusEquals),
                new Token(TokenType.Identifier, "d"),
                Helpers.GetDefaultToken(TokenType.OperatorAsteriskEquals),
                new Token(TokenType.Identifier, "e"),
                Helpers.GetDefaultToken(TokenType.OperatorSlashEquals),
                new Token(TokenType.Identifier, "f"),
                Helpers.GetDefaultToken(TokenType.OperatorPercentEquals),
                new Token(TokenType.Identifier, "g"),
                Helpers.GetDefaultToken(TokenType.OperatorSlashEquals),
                new Token(TokenType.Identifier, "h"),
                Helpers.GetDefaultToken(TokenType.OperatorAsteriskEquals),
                new Token(TokenType.Identifier, "i"),
                Helpers.GetDefaultToken(TokenType.OperatorMinusEquals),
                new Token(TokenType.Identifier, "j"),
                Helpers.GetDefaultToken(TokenType.OperatorPlusEquals),
                new Token(TokenType.Identifier, "k"),
                Helpers.GetDefaultToken(TokenType.OperatorEquals),
                new Token(TokenType.Identifier, "l"),
                Helpers.GetDefaultToken(TokenType.Semicolon)
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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
