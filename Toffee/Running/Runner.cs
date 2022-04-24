namespace Toffee.Running;

public class Runner : IRunner
{
    private readonly TextReader _reader;
    private readonly string _sourceName;

    public Runner(TextReader reader, string sourceName)
    {
        _reader = reader;
        _sourceName = sourceName;
    }

    public int Run()
    {
        // TODO: implement
        return 0;
    }
}
