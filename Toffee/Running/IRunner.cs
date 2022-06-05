using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public interface IRunner
{
    public void Run(Statement statement, EnvironmentStack? environmentStack);

    public object? Calculate(Expression expression, EnvironmentStack? environmentStack);
}
