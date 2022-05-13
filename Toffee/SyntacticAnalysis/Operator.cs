﻿namespace Toffee.SyntacticAnalysis;

public enum Operator
{
    NamespaceAccess,
    SafeNamespaceAccess,
    Exponentiation,
    NumberPromotion,
    ArithmeticNegation,
    LogicalNegation,
    Multiplication,
    Division,
    Remainder,
    Addition,
    Subtraction,
    Concatenation,
    LessThanComparison,
    LessOrEqualComparison,
    GreaterThanComparison,
    GreaterOrEqualComparison,
    EqualComparison,
    NotEqualComparison,
    PatternMatchingLessThanComparison,
    PatternMatchingLessOrEqualComparison,
    PatternMatchingGreaterThanComparison,
    PatternMatchingGreaterOrEqualComparison,
    PatternMatchingEqualComparison,
    PatternMatchingNotEqualComparison,
    EqualTypeCheck,
    NotEqualTypeCheck,
    PatternMatchingEqualTypeCheck,
    PatternMatchingNotEqualTypeCheck,
    Conjunction,
    Disjunction,
    PatternMatchingConjunction,
    PatternMatchingDisjunction,
    NullSafePipe,
    NullCoalescing,
    Assignment,
    AdditionAssignment,
    SubtractionAssignment,
    MultiplicationAssignment,
    DivisionAssignment,
    RemainderAssignment
}
