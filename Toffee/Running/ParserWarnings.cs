using System.Collections.Immutable;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.Running;

public abstract record RunnerWarning(Position Position) : Warning(Position);

public static class RunnerWarningExtensions
{
    private static readonly ImmutableDictionary<Type, string> MessageMap = ImmutableDictionary<Type, string>.Empty;

    public static string ToMessage(this RunnerWarning warning) =>
        MessageMap.GetValueOrDefault(warning.GetType(), "Runtime warning");
}
