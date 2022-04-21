namespace Toffee;

public class Runner
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
        return 0;
    }
}
