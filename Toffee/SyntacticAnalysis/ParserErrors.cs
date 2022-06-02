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
public record ExpectedSemicolon(Position Position, TokenType ActualType) : ParserError(Position)
{
    public ExpectedSemicolon(Token actualToken) : this(actualToken.StartPosition, actualToken.Type)
    { }
}
public record ExpectedIdentifier(Position Position, Type ActualType) : ParserError(Position)
{
    public ExpectedIdentifier(Expression actualExpression)
        : this(actualExpression.StartPosition, actualExpression.GetType())
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
        { typeof(ExpectedIdentifier), "Expected only identifiers in namespace path" }
    }.ToImmutableDictionary();

    public static string ToMessage(this ParserError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Syntax error");
}
