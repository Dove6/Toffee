using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record Expression(Position StartPosition, Position EndPosition);

public record BlockExpression(IList<Statement> Statements, Expression? ResultExpression = null)
    : Expression(new Position(), new Position());
public record ConditionalExpression(ConditionalElement IfPart, IList<ConditionalElement> ElifParts, Expression? ElsePart = null)
    : Expression(new Position(), new Position());
public record ForLoopExpression(ForLoopRange Range, Expression Body, string? CounterName = null)
    : Expression(new Position(), new Position());
public record WhileLoopExpression(Expression Condition, Expression Body)
    : Expression(new Position(), new Position());
public record FunctionDefinitionExpression(IList<FunctionParameter> Parameters, BlockExpression Body)
    : Expression(new Position(), new Position());
public record PatternMatchingExpression(Expression Argument, IList<PatternMatchingBranch> Branches, Expression? Default = null)
    : Expression(new Position(), new Position());

public record ConditionalElement(Expression Condition, Expression Consequent);
public record ForLoopRange(Expression PastTheEnd, Expression? Start = null, Expression? Step = null);
public record FunctionParameter(string Name, bool IsConst = false, bool IsNullAllowed = true);
public record PatternMatchingBranch(Expression? Pattern, Expression Consequent);

public record GroupingExpression(Expression Expression)
    : Expression(new Position(), new Position());
public record BinaryExpression(Expression Left, Operator Operator, Expression Right)
    : Expression(new Position(), new Position());
public record UnaryExpression(Operator Operator, Expression Expression)
    : Expression(new Position(), new Position());
public record FunctionCallExpression(Expression Expression, IList<Expression> Arguments)
    : Expression(new Position(), new Position());
public record IdentifierExpression(string Name)
    : Expression(new Position(), new Position());
public record LiteralExpression(DataType Type, object? Value)
    : Expression(new Position(), new Position());
public record TypeCastExpression(DataType Type, Expression Expression)
    : Expression(new Position(), new Position());
public record TypeExpression(DataType Type)
    : Expression(new Position(), new Position());
