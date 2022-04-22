namespace Toffee;

public readonly record struct Token(TokenType Type, object? Content, Position Position)
{
    public Token(TokenType type, object? content = null) : this(type, content, new Position())
    { }
}
