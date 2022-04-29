using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public class Parser : IParser
{
    private LexerBase _lexer;

    public Parser(LexerBase lexer)
    {
        _lexer = lexer;
    }
}
