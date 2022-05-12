namespace Toffee.SyntacticAnalysis;

public interface IExpression
{ }

public record BlockExpression(IList<IStatement> Statements, IStatement? UnterminatedStatement) : IExpression;
public record ConditionalExpression(ConditionalElement IfPart, IList<ConditionalElement> ElifParts, IExpression? ElsePart) : IExpression;
public record ForLoopExpression(string? CounterName, ForLoopRange Range, IExpression Body) : IExpression;
public record WhileLoopExpression(IExpression Condition, IExpression Body) : IExpression;
public record FunctionDefinitionExpression(IList<FunctionParameter> Parameters, BlockExpression Body) : IExpression;
public record PatternMatchingExpression(IExpression Argument, IList<PatternMatchingBranch> Branches, IExpression? Default) : IExpression;

public record ConditionalElement(IExpression Condition, IExpression Consequent);
public record ForLoopRange(IExpression PastTheEnd, IExpression? Start, IExpression? Step);
public record FunctionParameter(string Name, bool IsConst = false, bool IsNullAllowed = true);
public record PatternMatchingBranch(IExpression Pattern, IExpression Consequent);

public record BinaryExpression(IExpression Left, Operator Operator, IExpression Right) : IExpression;
public record UnaryExpression(IExpression Expression, Operator Operator) : IExpression;
public record FunctionCallExpression(IExpression Name, IList<IExpression> Arguments) : IExpression;
public record IdentifierExpression(IExpression Name) : IExpression;
public record LiteralExpression(object Value) : IExpression;
