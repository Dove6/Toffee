using CommandDotNet;
using CommandDotNet.NameCasing;
using Toffee.CommandLine;

return new AppRunner<Application>()
    .UseNameCasing(Case.KebabCase)
    .Run(args);
