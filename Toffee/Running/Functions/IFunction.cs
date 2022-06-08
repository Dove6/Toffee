namespace Toffee.Running.Functions;

public interface IFunction
{
    object? Call(IRunner runner, IList<object?> arguments);
}
