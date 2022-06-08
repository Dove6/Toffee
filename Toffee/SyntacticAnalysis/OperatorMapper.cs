using System.Collections.Immutable;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public static class OperatorMapper
{
    private static readonly ImmutableDictionary<TokenType, Operator> ComparisonMap = new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorLess, Operator.LessThanComparison },
        { TokenType.OperatorLessEquals, Operator.LessOrEqualComparison },
        { TokenType.OperatorGreater, Operator.GreaterThanComparison },
        { TokenType.OperatorGreaterEquals, Operator.GreaterOrEqualComparison },
        { TokenType.OperatorEqualsEquals, Operator.EqualComparison },
        { TokenType.OperatorBangEquals, Operator.NotEqualComparison }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<TokenType, Operator> PatternMatchingComparisonMap = new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorLess, Operator.LessThanComparison },
        { TokenType.OperatorLessEquals, Operator.LessOrEqualComparison },
        { TokenType.OperatorGreater, Operator.GreaterThanComparison },
        { TokenType.OperatorGreaterEquals, Operator.GreaterOrEqualComparison },
        { TokenType.OperatorEqualsEquals, Operator.EqualComparison },
        { TokenType.OperatorBangEquals, Operator.NotEqualComparison }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<TokenType, Operator> AssignmentMap = new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorEquals, Operator.Assignment },
        { TokenType.OperatorPlusEquals, Operator.AdditionAssignment },
        { TokenType.OperatorMinusEquals, Operator.SubtractionAssignment },
        { TokenType.OperatorAsteriskEquals, Operator.MultiplicationAssignment },
        { TokenType.OperatorSlashEquals, Operator.DivisionAssignment },
        { TokenType.OperatorPercentEquals, Operator.RemainderAssignment }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<TokenType, Operator> AdditiveMap = new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorPlus, Operator.Addition },
        { TokenType.OperatorMinus, Operator.Subtraction }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<TokenType, Operator> MultiplicativeMap = new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorAsterisk, Operator.Multiplication },
        { TokenType.OperatorSlash, Operator.Division },
        { TokenType.OperatorPercent, Operator.Remainder }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<TokenType, Operator> UnaryMap = new Dictionary<TokenType, Operator>
    {
        { TokenType.OperatorPlus, Operator.NumberPromotion },
        { TokenType.OperatorMinus, Operator.ArithmeticNegation },
        { TokenType.OperatorBang, Operator.LogicalNegation }
    }.ToImmutableDictionary();

    public static TokenType[] ComparisonTokenTypes { get; } = ComparisonMap.Keys.ToArray();
    public static TokenType[] PatternMatchingComparisonTokenTypes { get; } = PatternMatchingComparisonMap.Keys.ToArray();
    public static TokenType[] AssignmentTokenTypes { get; } = AssignmentMap.Keys.ToArray();
    public static TokenType[] AdditiveTokenTypes { get; } = AdditiveMap.Keys.ToArray();
    public static TokenType[] MultiplicativeTokenTypes { get; } = MultiplicativeMap.Keys.ToArray();
    public static TokenType[] UnaryTokenTypes { get; } = UnaryMap.Keys.ToArray();

    public static Operator MapComparisonOperator(TokenType tokenType) => ComparisonMap[tokenType];
    public static Operator MapPatternMatchingComparisonOperator(TokenType tokenType) =>
        PatternMatchingComparisonMap[tokenType];
    public static Operator MapAssignmentOperator(TokenType tokenType) => AssignmentMap[tokenType];
    public static Operator MapAdditiveOperator(TokenType tokenType) => AdditiveMap[tokenType];
    public static Operator MapMultiplicativeOperator(TokenType tokenType) => MultiplicativeMap[tokenType];
    public static Operator MapUnaryOperator(TokenType tokenType) => UnaryMap[tokenType];
}
