using System.Collections.ObjectModel;
using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record ParserError(Position Position) : Error(Position);

public record UnexpectedToken(Token ActualToken, params TokenType[] ExpectedType) : ParserError(ActualToken.StartPosition);
public record ExpectedStatement(Token ActualToken) : ParserError(ActualToken.StartPosition);
public record ExpectedExpression(Token ActualToken) : ParserError(ActualToken.StartPosition);
public record ExpectedBlockExpression(Token ActualToken) : ParserError(ActualToken.StartPosition);
public record ExpectedPatternExpression(Token ActualToken) : ParserError(ActualToken.StartPosition);

public static class ParserErrorExtensions
{
    private static readonly ReadOnlyDictionary<Type, string> MessageMap = new(new Dictionary<Type, string>
    {
        { typeof(UnexpectedToken), "Unexpected token" }
    });

    public static string ToMessage(this ParserError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Lexical error");
}
