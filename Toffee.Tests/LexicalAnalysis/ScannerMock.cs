using Toffee.Scanning;

namespace Toffee.Tests.LexicalAnalysis;

public class ScannerMock : IScanner
{
    private readonly string _outputBuffer;

    public char? CurrentCharacter => CurrentPosition.Character < _outputBuffer.Length
        ? _outputBuffer[(int)CurrentPosition.Character]
        : null;
    public Position CurrentPosition { get; private set; } = new();

    public ScannerMock(string contentToOutput)
    {
        _outputBuffer = contentToOutput;
    }

    public void Advance()
    {
        if (CurrentCharacter is '\n')
            CurrentPosition = CurrentPosition.WithIncrementedLine(1);
        else if (CurrentCharacter is not null)
            CurrentPosition = CurrentPosition.WithIncrementedColumn();
    }
}
