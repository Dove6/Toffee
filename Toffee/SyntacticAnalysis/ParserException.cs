namespace Toffee.SyntacticAnalysis;

public class ParserException : Exception
{
    private readonly ParserError _error;

    public ParserException(ParserError error) =>
        _error = error;

    public ParserException(ParserError error, string message) : base(message) =>
        _error = error;

    public ParserException(ParserError error, string message, Exception inner) : base(message, inner) =>
        _error = error;
}
