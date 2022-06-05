using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public interface IRunner
{
    public void Run(Statement statement);

    public object? Calculate(Expression expression);
}
