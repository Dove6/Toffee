namespace Toffee.Scanning;

public interface IScanner
{
    char? CurrentCharacter { get; }
    Position CurrentPosition { get; }

    void Advance();
}
