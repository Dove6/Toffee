namespace Toffee.Scanning;

public class Scanner : IScanner
{
    private readonly TextReader _reader;

    private const int BufferCapacity = 2;
    private readonly List<char> _buffer = new(BufferCapacity);

    public char? CurrentCharacter { get; private set; }
    public Position CurrentPosition { get; private set; } = new();
    private Position _nextPosition = new();
    private Position NextPosition
    {
        get => _nextPosition;
        set
        {
            CurrentPosition = _nextPosition;
            _nextPosition = value;
        }
    }

    public Scanner(TextReader reader)
    {
        _reader = reader;
        Advance();
    }

    private void RestockBuffer()
    {
        for (var i = _buffer.Count; i < BufferCapacity; i++)
        {
            var readCharacter = _reader.Read();
            if (readCharacter == -1)
                break;
            _buffer.Add((char)readCharacter);
        }
    }

    private bool TryMatchNewLine(out uint length)
    {
        length = (_buffer[0], (char?)(_buffer.Count > 1 ? _buffer[1] : null)) switch
        {
            ('\n', '\r') => 2,
            ('\n', _)    => 1,
            ('\r', '\n') => 2,
            ('\r', _)    => 1,
            ('\x1e', _)  => 1,
            (_, _)       => 0
        };
        return length > 0;
    }

    public void Advance()
    {
        RestockBuffer();
        if (_buffer.Count == 0)
        {
            NextPosition = NextPosition with {};
            CurrentCharacter = null;
            return;
        }

        if (TryMatchNewLine(out var newLineSequenceLength))
        {
            NextPosition = NextPosition.WithIncrementedLine(newLineSequenceLength);
            CurrentCharacter = '\n';
            _buffer.RemoveRange(0, (int)newLineSequenceLength);
        }
        else
        {
            NextPosition = NextPosition.WithIncrementedColumn;
            CurrentCharacter = _buffer[0];
            _buffer.RemoveAt(0);
        }
    }
}
