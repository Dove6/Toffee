using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public interface IRunner
{
    void Run(Statement statement, EnvironmentStack? environmentStack);

    object? Calculate(Expression expression, EnvironmentStack? environmentStack);

    bool ShouldQuit { get; set; }
}
