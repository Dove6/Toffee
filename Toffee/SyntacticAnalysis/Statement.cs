using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record Statement(Position StartPosition, Position EndPosition, bool IsTerminated = false);

public record NamespaceImportStatement(IList<string> NamespaceLevels)
    : Statement(new Position(), new Position());
public record VariableInitializationListStatement(IList<VariableInitialization> Items)
    : Statement(new Position(), new Position());
public record BreakStatement(Expression? Condition = null)
    : Statement(new Position(), new Position());
public record ReturnStatement(Expression? Value = null)
    : Statement(new Position(), new Position());
public record ExpressionStatement(Expression Expression)
    : Statement(new Position(), new Position());

public record VariableInitialization(string Name, Expression? InitialValue = null, bool IsConst = false, Position? Position = null);
