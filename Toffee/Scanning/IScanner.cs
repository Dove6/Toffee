namespace Toffee.Scanning;

public interface IScanner
{
    char? CurrentCharacter { get; }
    Position CurrentPosition { get; }

    /// <summary>
    /// Advances the position of the scanner in the character stream.
    /// </summary>
    /// <returns>Superseded character - the current one from before the method was called</returns>
    char? Advance();
}
