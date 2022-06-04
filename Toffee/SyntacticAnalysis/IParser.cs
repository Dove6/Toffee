namespace Toffee.SyntacticAnalysis;

public interface IParser  // TODO: implement IEnumerable?
{
    /// <summary>
    /// Current top-level statement.
    /// If null, there are no statements left to parse.
    /// </summary>
    Statement? CurrentStatement { get; }

    /// <summary>
    /// Advances the position of the parser in the statement stream.
    /// </summary>
    /// <returns>Parsed statement - the current one after the method was called</returns>
    Statement? Advance();
}
