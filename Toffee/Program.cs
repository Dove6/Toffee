using CommandDotNet;
using CommandDotNet.NameCasing;
using Toffee.CommandLine;

namespace Toffee;

public static class Program
{
    private static int Main(string[] args) => AppRunner.Run(args);

    public static AppRunner AppRunner =>
        new AppRunner<Application>()
            .UseNameCasing(Case.KebabCase);
}
