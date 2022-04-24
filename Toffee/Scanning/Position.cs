namespace Toffee.Scanning;

public record Position(uint Character = 0, uint Line = 1, uint Column = 0)
{
    public Position WithIncrementedLine(uint newLineSequenceLength) => new()
    {
        Character = Character + newLineSequenceLength,
        Line = Line + 1,
        Column = 0
    };

    public Position WithIncrementedColumn() => new()
    {
        Character = Character + 1,
        Line = Line,
        Column = Column + 1
    };
}
