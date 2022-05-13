namespace Toffee.SyntacticAnalysis;

public interface IExpression
{ }

public record BlockExpression(IList<IStatement> Statements, IStatement? UnterminatedStatement = null) : IExpression;
public record ConditionalExpression(ConditionalElement IfPart, IList<ConditionalElement> ElifParts,
    IExpression? ElsePart = null) : IExpression;
public record ForLoopExpression(ForLoopRange Range, IExpression Body, string? CounterName = null) : IExpression;
public record WhileLoopExpression(IExpression Condition, IExpression Body) : IExpression;
public record FunctionDefinitionExpression(IList<FunctionParameter> Parameters, BlockExpression Body) : IExpression;
public record PatternMatchingExpression(IExpression Argument, IList<PatternMatchingBranch> Branches,
    IExpression? Default = null) : IExpression;

public record ConditionalElement(IExpression Condition, IExpression Consequent);
public record ForLoopRange(IExpression PastTheEnd, IExpression? Start = null, IExpression? Step = null);
public record FunctionParameter(string Name, bool IsConst = false, bool IsNullAllowed = true);
public record PatternMatchingBranch(IExpression Pattern, IExpression Consequent);

public record BinaryExpression(IExpression Left, Operator Operator, IExpression Right) : IExpression;
public record UnaryExpression(IExpression Expression, Operator Operator) : IExpression;
public record FunctionCallExpression(IExpression Name, IList<IExpression> Arguments) : IExpression;
public record IdentifierExpression(IExpression Name) : IExpression;
public record LiteralExpression(object Value) : IExpression;
