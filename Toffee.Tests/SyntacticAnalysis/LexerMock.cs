using System;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;

namespace Toffee.Tests.SyntacticAnalysis;

public class LexerMock : ILexer
{
    private readonly Token[] _outputBuffer;
    private int _currentOutputIndex;

    private Position CurrentPosition => new((uint)_currentOutputIndex, 1, (uint)_currentOutputIndex);

    public int MaxLexemeLength
    {
        get => int.MaxValue;
        set => throw new NotSupportedException();
    }

    public LexerError? CurrentError => null;

    public Token CurrentToken => (_currentOutputIndex < _outputBuffer.Length
            ? _outputBuffer[_currentOutputIndex]
            : new Token(TokenType.EndOfText, "ETX"))
        with { StartPosition = CurrentPosition, EndPosition = CurrentPosition.WithIncrementedColumn() };

    public LexerMock(params Token[] outputBuffer)
    {
        _outputBuffer = outputBuffer;
    }

    public Token Advance()
    {
        var supersededToken = CurrentToken;
        _currentOutputIndex++;
        return supersededToken;
    }
}
