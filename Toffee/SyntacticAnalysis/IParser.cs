namespace Toffee.SyntacticAnalysis;

public interface IParser
{
    /// <summary>
    /// Current top-level statement.
    /// If null, there are no statements left to parse.
    /// </summary>
    Statement? CurrentStatement { get; }

    /// <summary>
    /// Advances the position of the parser in the statement stream.
    /// </summary>
    /// <returns>Superseded statement - the current one from before the method was called</returns>
    Statement? Advance();
}
