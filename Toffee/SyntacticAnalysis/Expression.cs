using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record Expression(Position StartPosition, Position EndPosition);

public record BlockExpression(IList<Statement> Statements, Expression? ResultExpression = null)
    : Expression(new Position(), new Position())
{
    public BlockExpression(Statement statement) : this(new List<Statement> { statement })
    { }
    public BlockExpression(Expression expression) : this(new List<Statement>(), expression)
    { }
}
public record ConditionalExpression(IList<ConditionalElement> Branches, BlockExpression? ElseBranch = null)
    : Expression(new Position(), new Position());
public record ForLoopExpression(ForLoopRange Range, BlockExpression Body, string? CounterName = null)
    : Expression(new Position(), new Position());
public record WhileLoopExpression(Expression Condition, BlockExpression Body)
    : Expression(new Position(), new Position());
public record FunctionDefinitionExpression(IList<FunctionParameter> Parameters, BlockExpression Body)
    : Expression(new Position(), new Position());
public record PatternMatchingExpression(Expression Argument, IList<PatternMatchingBranch> Branches, BlockExpression? DefaultBranch = null)
    : Expression(new Position(), new Position());

public record ConditionalElement(Expression Condition, BlockExpression Consequent);
public record ForLoopRange(Expression PastTheEnd, Expression? Start = null, Expression? Step = null);
public record FunctionParameter(string Name, bool IsConst = false, bool IsNullAllowed = true);
public record PatternMatchingBranch(Expression? Pattern, BlockExpression Consequent);

public record GroupingExpression(Expression Expression)
    : Expression(new Position(), new Position());
public record BinaryExpression(Expression Left, Operator Operator, Expression Right)
    : Expression(new Position(), new Position());
public record UnaryExpression(Operator Operator, Expression Expression)
    : Expression(new Position(), new Position());
public record FunctionCallExpression(Expression Callee, IList<Expression> Arguments)
    : Expression(new Position(), new Position());
public record IdentifierExpression(IList<string> NamespaceLevels, string Name)
    : Expression(new Position(), new Position())
{
    public IdentifierExpression(string name) : this(new List<string>(), name)
    { }
}
public record LiteralExpression(DataType Type, object? Value)
    : Expression(new Position(), new Position());
public record TypeCastExpression(DataType Type, Expression Expression)
    : Expression(new Position(), new Position());
public record TypeCheckExpression(Expression Expression, DataType Type, bool IsInequalityCheck = false)
    : Expression(new Position(), new Position());
public record PatternTypeCheckExpression(DataType Type, bool IsInequalityCheck = false)
    : Expression(new Position(), new Position());
