namespace Toffee.SyntacticAnalysis;

public abstract record Expression;  // TODO: add Position

public record BlockExpression(IList<Statement> Statements, Statement? UnterminatedStatement = null) : Expression;
public record ConditionalExpression(ConditionalElement IfPart, IList<ConditionalElement> ElifParts,
    Expression? ElsePart = null) : Expression;
public record ForLoopExpression(ForLoopRange Range, Expression Body, string? CounterName = null) : Expression;
public record WhileLoopExpression(Expression Condition, Expression Body) : Expression;
public record FunctionDefinitionExpression(IList<FunctionParameter> Parameters, BlockExpression Body) : Expression;
public record PatternMatchingExpression(Expression Argument, IList<PatternMatchingBranch> Branches,
    Expression? Default = null) : Expression;

public record ConditionalElement(Expression Condition, Expression Consequent);
public record ForLoopRange(Expression PastTheEnd, Expression? Start = null, Expression? Step = null);
public record FunctionParameter(string Name, bool IsConst = false, bool IsNullAllowed = true);
public record PatternMatchingBranch(Expression? Pattern, Expression Consequent);

public record BinaryExpression(Expression Left, Operator Operator, Expression Right) : Expression;
public record UnaryExpression(Operator Operator, Expression Expression) : Expression;
public record FunctionCallExpression(Expression Expression, IList<Expression> Arguments) : Expression;
public record IdentifierExpression(string Name) : Expression;
public record LiteralExpression(DataType Type, object? Value) : Expression;
public record TypeExpression(DataType Type) : Expression;
// TODO: GroupingExpression
