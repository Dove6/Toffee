namespace Toffee.SyntacticAnalysis;

public abstract record Expression;

public record BlockExpression(IList<Statement> Statements, Statement? UnterminatedStatement = null) : Expression;
public record ConditionalExpression(ConditionalElement IfPart, IList<ConditionalElement> ElifParts,
    Statement? ElsePart = null) : Expression;
public record ForLoopExpression(ForLoopRange Range, Statement Body, string? CounterName = null) : Expression;
public record WhileLoopExpression(Expression Condition, Statement Body) : Expression;
public record FunctionDefinitionExpression(IList<FunctionParameter> Parameters, BlockExpression Body) : Expression;
public record PatternMatchingExpression(Expression Argument, IList<PatternMatchingBranch> Branches,
    Expression? Default = null) : Expression;

public record ConditionalElement(Expression Condition, Statement Consequent);
public record ForLoopRange(Expression PastTheEnd, Expression? Start = null, Expression? Step = null);
public record FunctionParameter(string Name, bool IsConst = false, bool IsNullAllowed = true);
public record PatternMatchingBranch(Expression? Pattern, Expression Consequent);

public record BinaryExpression(Expression Left, Operator Operator, Expression Right) : Expression;
public record UnaryExpression(Expression Expression, Operator Operator) : Expression;
public record FunctionCallExpression(Expression Name, IList<Expression> Arguments) : Expression;
public record IdentifierExpression(Expression Name) : Expression;
public record LiteralExpression(object Value) : Expression;
