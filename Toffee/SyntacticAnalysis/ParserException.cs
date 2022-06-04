namespace Toffee.SyntacticAnalysis;

public class ParserException : Exception
{
    public ParserError Error { get; }

    public ParserException(ParserError error) =>
        Error = error;

    public ParserException(ParserError error, string message) : base(message) =>
        Error = error;

    public ParserException(ParserError error, string message, Exception inner) : base(message, inner) =>
        Error = error;
}
