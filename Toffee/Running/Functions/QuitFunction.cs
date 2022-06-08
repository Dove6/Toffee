namespace Toffee.Running.Functions;

public class QuitFunction : IFunction
{
    public object? Call(IRunner runner, IList<object?> arguments)
    {
        runner.ShouldQuit = true;
        return null;
    }
}
