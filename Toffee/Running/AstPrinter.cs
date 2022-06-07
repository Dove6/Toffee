namespace Toffee.Running;

public partial class AstPrinter
{
    private readonly string _inputName;
    private readonly TextWriter _textWriter;

    public AstPrinter(string inputName, TextWriter? writer)
    {
        _inputName = inputName;
        _textWriter = writer ?? Console.Out;
    }

    private void Print(string text, int indentLevel = 0)
    {
        var indentation = new string(' ', indentLevel * 2);
        text.Split(new[] { '\r', '\n', '\xe1' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            .ForEach(x =>
            {
                _textWriter.Write(indentation);
                _textWriter.WriteLine(x);
            });
    }
}
