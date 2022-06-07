namespace Toffee.SyntacticAnalysis;

public interface IParser  // TODO: implement IEnumerable?
{
    /// <summary>
    /// Tries to parse a statement.
    /// </summary>
    /// <param name="parsedStatement">Parsed statement</param>
    /// <returns>if parsing was successful</returns>
    bool TryAdvance(out Statement? parsedStatement);
}
