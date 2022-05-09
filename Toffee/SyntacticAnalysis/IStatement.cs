﻿namespace Toffee.SyntacticAnalysis;

public interface IStatement
{ }

public record NamespaceImportStatement(IList<string> NamespaceLevels) : IStatement;
public record VariableInitializationStatement(string Name, IExpression? InitialValue, bool IsConst = false) : IStatement;
public record BreakStatement : IStatement;
public record BreakIfStatement(IExpression Condition) : IStatement;
public record ReturnStatement(IExpression? Value) : IStatement;
public record ExpressionStatement(IExpression Expression) : IStatement;
