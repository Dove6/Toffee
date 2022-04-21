using Toffee;

if (args.Length > 1)
{
    Console.WriteLine("Usage: Toffee [script]");
    return 64;
}
else if (args.Length == 1)
{
    using var reader = new StreamReader(args[0]);
    var runner = new Runner(reader, args[0]);
    return runner.Run();
}
else
{
    var runner = new Runner(Console.In, "STDIN");
    return runner.Run();
}
