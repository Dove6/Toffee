﻿namespace Toffee.SyntacticAnalysis;

public abstract record Statement(bool IsTerminated = false);

public record NamespaceImportStatement(IList<string> NamespaceLevels) : Statement;
public record VariableInitializationListStatement(IList<VariableInitialization> Items) : Statement;
public record BreakStatement : Statement;
public record BreakIfStatement(Expression Condition) : Statement;
public record ReturnStatement(Expression? Value = null) : Statement;
public record ExpressionStatement(Expression Expression) : Statement;

public record VariableInitialization(string Name, Expression? InitialValue = null, bool IsConst = false) : Statement;
