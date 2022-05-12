namespace Toffee.Running;

public static partial class AstPrinter
{
    private static void Print(string text, int indentLevel = 0)
    {
        var indentation = new string(' ', indentLevel * 2);
        text.Split(new[] { '\r', '\n', '\xe1' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            .ForEach(x =>
            {
                Console.Write(indentation);
                Console.WriteLine(x);
            });
    }
}
