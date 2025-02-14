﻿using System.Collections.Immutable;
using System.Text;
using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record ParserError(Position Position) : Error(Position);

public record UnexpectedToken(Position Position, TokenType ActualType, params TokenType[] ExpectedType)
    : ParserError(Position)
{
    public UnexpectedToken(Token actualToken, params TokenType[] expectedType)
        : this(actualToken.StartPosition, actualToken.Type, expectedType)
    { }
    protected override bool PrintMembers(StringBuilder stringBuilder)
    {
        if (base.PrintMembers(stringBuilder))
            stringBuilder.Append(", ");
        stringBuilder.Append($"{nameof(ActualType)} = {ActualType}, {nameof(ExpectedType)} = ");
        if (ExpectedType.Length > 1)
            stringBuilder.Append("[ ");
        if (ExpectedType.Length > 0)
            stringBuilder.Append($"{ExpectedType[0]}");
        else
            stringBuilder.Append($"[]");
        foreach (var type in ExpectedType.Skip(1))
            stringBuilder.Append($", {type}");
        if (ExpectedType.Length > 1)
            stringBuilder.Append(" ]");
        return true;
    }
}
public record ExpectedStatement(Position Position, TokenType ActualType) : ParserError(Position)
{
    public ExpectedStatement(Token actualToken) : this(actualToken.StartPosition, actualToken.Type)
    { }
}
public record ExpectedExpression(Position Position, TokenType ActualType) : ParserError(Position)
{
    public ExpectedExpression(Token actualToken) : this(actualToken.StartPosition, actualToken.Type)
    { }
}
public record ExpectedBlockExpression(Position Position, TokenType ActualType) : ParserError(Position)
{
    public ExpectedBlockExpression(Token actualToken) : this(actualToken.StartPosition, actualToken.Type)
    { }
}
public record ExpectedPatternExpression(Position Position, TokenType ActualType) : ParserError(Position)
{
    public ExpectedPatternExpression(Token actualToken) : this(actualToken.StartPosition, actualToken.Type)
    { }
}
public record ExpectedSemicolon(Position Position, TokenType? ActualTokenType = null, Type? ActualType = null)
    : ParserError(Position)
{
    public ExpectedSemicolon(Token actualToken) : this(actualToken.StartPosition, actualToken.Type)
    { }

    public ExpectedSemicolon(Statement actualStatement)
        : this(actualStatement.StartPosition, null, actualStatement.GetType())
    { }
    protected override bool PrintMembers(StringBuilder stringBuilder)
    {
        if (!base.PrintMembers(stringBuilder))
            return true;
        if (ActualTokenType is not null)
            stringBuilder.Append($", {nameof(ActualTokenType)} = {ActualTokenType}");
        if (ActualType is not null)
            stringBuilder.Append($", {nameof(ActualType)} = {ActualType.Name}");
        return true;
    }
}
public record IntegerOutOfRange(Position Position, ulong Value, bool Negative = false) : ParserError(Position)
{
    public IntegerOutOfRange(LiteralExpression actualExpression)
        : this(actualExpression.StartPosition, (ulong)actualExpression.Value!)
    { }
}
public record ExpectedParameter(Position Position, TokenType ActualType) : ParserError(Position)
{
    public ExpectedParameter(Token actualToken) : this(actualToken.StartPosition, actualToken.Type)
    { }
}
public record BranchAfterDefaultPattern(Position Position) : ParserError(Position)
{
    public BranchAfterDefaultPattern(Expression pattern) : this(pattern.StartPosition)
    { }
}
public record DuplicatedDefaultPattern(Position Position) : ParserError(Position)
{
    public DuplicatedDefaultPattern(Expression consequent) : this(consequent.StartPosition)
    { }
}
public record ImplicitConstInitialization(Position Position, string Name) : ParserError(Position)
{
    public ImplicitConstInitialization(VariableInitialization initialization)
        : this(initialization.Position ?? new Position(), initialization.Name)
    { }
}
public record DuplicatedParameterName (Position Position, string Name, int ParameterIndex, int PreviousIndex)
    : ParserError(Position)
{
    public DuplicatedParameterName(FunctionParameter parameter, int parameterIndex, int previousIndex)
        : this(parameter.Position ?? new Position(), parameter.Name, parameterIndex, previousIndex)
    { }
}
public record LexicalError(Position Position) : ParserError(Position)
{
    public LexicalError(LexerError error) : this(error.Position)
    { }
}

public static class ParserErrorExtensions
{
    private static readonly ImmutableDictionary<Type, string> MessageMap = new Dictionary<Type, string>
    {
        { typeof(UnexpectedToken), "Unexpected token" },
        { typeof(ExpectedStatement), "Unexpected token instead of a statement" },
        { typeof(ExpectedExpression), "Unexpected token instead of an expression" },
        { typeof(ExpectedBlockExpression), "Unexpected token instead of a block expression" },
        { typeof(ExpectedPatternExpression), "Unexpected token instead of a pattern expression" },
        { typeof(ExpectedSemicolon), "Expected terminating semicolon" },
        { typeof(IntegerOutOfRange), "Literal integer above maximum (positive) value or below minimum (negative) value" },
        { typeof(ExpectedParameter), "Expected parameter in parameter list" },
        { typeof(BranchAfterDefaultPattern), "No branches should follow the default pattern" },
        { typeof(DuplicatedDefaultPattern), "Default pattern cannot be used more than once" },
        { typeof(ImplicitConstInitialization), "Const requires an initial value" },
        { typeof(DuplicatedParameterName), "Function parameters should have unique names" },
        { typeof(LexicalError), "Lexical error" }
    }.ToImmutableDictionary();

    public static string ToMessage(this ParserError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Syntax error");
}
