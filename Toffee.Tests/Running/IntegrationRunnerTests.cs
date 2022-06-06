using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;
using Toffee.Running;
using Toffee.Running.Functions;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;
using Xunit;

namespace Toffee.Tests.Running;

public class IntegrationRunnerTests
{
    private string RunText(string text)
    {
        var writer = new StringWriter();
        writer.NewLine = "\n";
        using var reader = new StringReader(text);
        IScanner scanner = new Scanner(reader);
        var logger = new ConsoleErrorHandler("test");
        ILexer lexer = new Lexer(scanner, logger);
        IParser parser = new Parser(lexer, logger);
        var runner = new Runner(logger, new EnvironmentStack(new Environment(new Dictionary<string, Variable>
        {
            { "print", new Variable(new PrintFunction(writer), false) }
        })));
        while (!runner.ShouldQuit && parser.Advance() is not null)
            runner.Run(parser.CurrentStatement!);
        return writer.ToString();
    }

    private static string Join(IEnumerable<string> textLines) =>
        string.Join("", textLines.SelectMany(x => new[] { x, "\n" }));

    [Fact]
    public void VariablesShouldBeMutableByDefault()
    {
        var testText = Join(new[]
        {
            "init a = 5;",
            "print(a);",
            "a = 6;",
            "print(a);"
        });
        var expectedOutput = Join(new[]
        {
            "5",
            "6"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void VariablesShouldBeImmutableUsingConst()
    {
        var testText = Join(new[]
        {
            "init const a = 5;",
            "print(a);",
            "a = 6;",
            "print(a);"
        });
        var expectedOutput = Join(new[]
        {
            "5",
            "5"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void VariablesShouldBeOptional()
    {
        var testText = Join(new[]
        {
            "init a = 5;",
            "print(a);",
            "a = null;",
            "print(a);"
        });
        var expectedOutput = Join(new[]
        {
            "5",
            "null"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void VariablesShouldBeAccessibleFromInsideBlocks()
    {
        var testText = Join(new[]
        {
            "init a = 5;",
            "{",
            "    print(a);",
            "    a = 6;",
            "    print(a);",
            "};",
            "print(a);"
        });
        var expectedOutput = Join(new[]
        {
            "5",
            "6",
            "6"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void VariablesShouldBeAccessibleFromInsideFunctions()
    {
        var testText = Join(new[]
        {
            "init a = 5;",
            "(functi() {",
            "    print(a);",
            "    a = 6;",
            "    print(a);",
            "})();",
            "print(a);"
        });
        var expectedOutput = Join(new[]
        {
            "5",
            "6",
            "6"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void ExtendingCapturedScopeShoulNotBePossible()
    {
        var testText = Join(new[]
        {
            "init a = \"global\";",
            "{",
            "    init show = functi() print(a);",
            "    show();",
            "    init a = \"block\";",
            "    show();",
            "};"
        });
        var expectedOutput = Join(new[]
        {
            "global",
            "global"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void CreatingClosureCounterShouldBePossible()
    {
        var testText = Join(new[]
        {
            "init makeCounter = functi() {",
            "    init i = 0;",
            "    functi() i += 1",
            "};",
            "init counter = makeCounter();",
            "print(counter());",
            "print(counter());",
            "print(makeCounter()());"
        });
        var expectedOutput = Join(new[]
        {
            "1",
            "2",
            "1"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void CommentsShouldBeIgnored()
    {
        var testText = Join(new[]
        {
            "// line comment",
            "/* block",
            "   comment */"
        });
        var expectedOutput = "";
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }
}
