using System.Collections.ObjectModel;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public static class OperatorMapper
{
    private static readonly ReadOnlyDictionary<TokenType, Operator> ComparisonMap = new(new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorLess, Operator.LessThanComparison },
        { TokenType.OperatorLessEquals, Operator.LessOrEqualComparison },
        { TokenType.OperatorGreater, Operator.GreaterThanComparison },
        { TokenType.OperatorGreaterEquals, Operator.GreaterOrEqualComparison },
        { TokenType.OperatorEqualsEquals, Operator.EqualComparison },
        { TokenType.OperatorBangEquals, Operator.NotEqualComparison }
    });

    private static readonly ReadOnlyDictionary<TokenType, Operator> PatternMatchingComparisonMap = new(new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorLess, Operator.PatternMatchingLessThanComparison },
        { TokenType.OperatorLessEquals, Operator.PatternMatchingLessOrEqualComparison },
        { TokenType.OperatorGreater, Operator.PatternMatchingGreaterThanComparison },
        { TokenType.OperatorGreaterEquals, Operator.PatternMatchingGreaterOrEqualComparison },
        { TokenType.OperatorEqualsEquals, Operator.PatternMatchingEqualComparison },
        { TokenType.OperatorBangEquals, Operator.PatternMatchingNotEqualComparison }
    });

    private static readonly ReadOnlyDictionary<TokenType, Operator> AssignmentMap = new(new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorEquals, Operator.Assignment },
        { TokenType.OperatorPlusEquals, Operator.AdditionAssignment },
        { TokenType.OperatorMinusEquals, Operator.SubtractionAssignment },
        { TokenType.OperatorAsteriskEquals, Operator.MultiplicationAssignment },
        { TokenType.OperatorSlashEquals, Operator.DivisionAssignment },
        { TokenType.OperatorPercentEquals, Operator.RemainderAssignment }
    });

    private static readonly ReadOnlyDictionary<TokenType, Operator> AdditiveMap = new(new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorPlus, Operator.Addition },
        { TokenType.OperatorMinus, Operator.Subtraction }
    });

    private static readonly ReadOnlyDictionary<TokenType, Operator> MultiplicativeMap = new(new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorAsterisk, Operator.Multiplication },
        { TokenType.OperatorSlash, Operator.Division },
        { TokenType.OperatorPercent, Operator.Remainder }
    });

    private static readonly ReadOnlyDictionary<TokenType, Operator> UnaryMap = new(new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorPlus, Operator.NumberPromotion },
        { TokenType.OperatorMinus, Operator.ArithmeticNegation },
        { TokenType.OperatorBang, Operator.LogicalNegation }
    });

    public static Operator MapComparisonOperator(TokenType tokenType) => ComparisonMap[tokenType];  // TODO: throws
    public static Operator MapPatternMatchingComparisonOperator(TokenType tokenType) =>
        PatternMatchingComparisonMap[tokenType];  // TODO: throws
    public static Operator MapAssignmentOperator(TokenType tokenType) => AssignmentMap[tokenType];  // TODO: throws
    public static Operator MapAdditiveOperator(TokenType tokenType) => AdditiveMap[tokenType];  // TODO: throws
    public static Operator MapMultiplicativeOperator(TokenType tokenType) => MultiplicativeMap[tokenType];  // TODO: throws
    public static Operator MapUnaryOperator(TokenType tokenType) => UnaryMap[tokenType];  // TODO: throws
}
