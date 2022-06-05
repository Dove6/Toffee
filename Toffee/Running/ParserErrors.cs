using System.Collections.Immutable;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.Running;

public abstract record RunnerError(Position Position) : Error(Position);

public static class RunnerErrorExtensions
{
    private static readonly ImmutableDictionary<Type, string> MessageMap = ImmutableDictionary<Type, string>.Empty;

    public static string ToMessage(this RunnerError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Runtime error");
}
