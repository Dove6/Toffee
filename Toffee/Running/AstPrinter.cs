namespace Toffee.Running;

public partial class AstPrinter
{
    private readonly string _inputName;

    public AstPrinter(string inputName)
    {
        _inputName = inputName;
    }

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
