namespace Toffee.SyntacticAnalysis;

public interface IParser  // TODO: implement IEnumerable?
{
    /// <summary>
    /// Tries to parse a statement.
    /// </summary>
    /// <param name="parsedStatement">Parsed statement (if no errors occured)</param>
    /// <param name="hadError">If any error occured while parsing the statement</param>
    /// <returns>if there was any statement to parse (ETX not reached)</returns>
    bool TryAdvance(out Statement? parsedStatement, out bool hadError);
}
