using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public class Parser : IParser
{
    private BaseLexer _lexer;

    public Parser(BaseLexer lexer)
    {
        _lexer = lexer;
    }

    public Program Parse()
    {
        throw new NotImplementedException();
    }
}
