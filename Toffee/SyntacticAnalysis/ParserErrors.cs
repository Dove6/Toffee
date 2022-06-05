using System.Collections.Immutable;
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
        { typeof(ExpectedParameter), "Expected parameter in parameter list" }
    }.ToImmutableDictionary();

    public static string ToMessage(this ParserError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Syntax error");
}
