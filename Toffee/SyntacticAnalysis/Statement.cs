using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record Statement(Position Position, bool IsTerminated = false);

public record NamespaceImportStatement(IList<IdentifierExpression> NamespaceLevels) : Statement(new Position());
public record VariableInitializationListStatement(IList<VariableInitialization> Items) : Statement(new Position());
public record BreakStatement() : Statement(new Position());
public record BreakIfStatement(Expression Condition) : Statement(new Position());
public record ReturnStatement(Expression? Value = null) : Statement(new Position());
public record ExpressionStatement(Expression Expression) : Statement(new Position());

public record VariableInitialization(string Name, Expression? InitialValue = null, bool IsConst = false) : Statement(new Position());
