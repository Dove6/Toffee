﻿namespace Toffee.Scanning;

public readonly record struct Position(uint Character, uint Line, uint Column)
{
    public Position() : this(0, 1, 0)
    { }

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
