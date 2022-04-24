using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public class Runner : IRunner
{
    private readonly IParser _parser;

    public Runner(IParser parser)
    {
        _parser = parser;
    }
}
