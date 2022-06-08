using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public interface IRunner
{
    void Run(Statement statement, EnvironmentStack? environmentStack = null);

    object? Calculate(Expression expression, EnvironmentStack? environmentStack = null);

    bool ShouldQuit { get; set; }
}
