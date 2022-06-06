using System.Collections.Generic;
using System.IO;
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

    [Fact]
    public void VariablesShouldBeMutableByDefault()
    {
        var testText = new[]
        {
            "init a = 5;",
            "print(a);",
            "a = 6;",
            "print(a);"
        };
        var testTextWithNewlines = string.Join("\n", testText);
        var expectedOutput = "5\r\n6\r\n";
        var output = RunText(testTextWithNewlines);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void VariablesShouldBeImmutableUsingConst()
    {
        var testText = new[]
        {
            "init const a = 5;",
            "print(a);",
            "a = 6;",
            "print(a);"
        };
        var testTextWithNewlines = string.Join("\n", testText);
        var expectedOutput = "5\r\n5\r\n";
        var output = RunText(testTextWithNewlines);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void VariablesShouldBeOptional()
    {
        var testText = new[]
        {
            "init a = 5;",
            "print(a);",
            "a = null;",
            "print(a);"
        };
        var testTextWithNewlines = string.Join("\n", testText);
        var expectedOutput = "5\r\nnull\r\n";
        var output = RunText(testTextWithNewlines);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void VariablesShouldBeAccessibleFromInsideBlocks()
    {
        var testText = new[]
        {
            "init a = 5;",
            "{",
            "    print(a);",
            "    a = 6;",
            "    print(a);",
            "};",
            "print(a);"
        };
        var testTextWithNewlines = string.Join("\n", testText);
        var expectedOutput = "5\r\n6\r\n6\r\n";
        var output = RunText(testTextWithNewlines);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void VariablesShouldBeAccessibleFromInsideFunctions()
    {
        var testText = new[]
        {
            "init a = 5;",
            "(functi() {",
            "    print(a);",
            "    a = 6;",
            "    print(a);",
            "})();",
            "print(a);"
        };
        var testTextWithNewlines = string.Join("\n", testText);
        var expectedOutput = "5\r\n6\r\n6\r\n";
        var output = RunText(testTextWithNewlines);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void ExtendingCapturedScopeShoulNotBePossible()
    {
        var testText = new[]
        {
            "init a = \"global\";",
            "{",
            "    init show = functi() print(a);",
            "    show();",
            "    init a = \"block\";",
            "    show();",
            "};"
        };
        var testTextWithNewlines = string.Join("\n", testText);
        var expectedOutput = "global\r\nglobal\r\n";
        var output = RunText(testTextWithNewlines);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void CreatingClosureCounterShouldBePossible()
    {
        var testText = new[]
        {
            "init makeCounter = functi() {",
            "    init i = 0;",
            "    functi() i += 1",
            "};",
            "init counter = makeCounter();",
            "print(counter());",
            "print(counter());",
            "print(makeCounter()());"
        };
        var testTextWithNewlines = string.Join("\n", testText);
        var expectedOutput = "1\r\n2\r\n1\r\n";
        var output = RunText(testTextWithNewlines);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void CommentsShouldBeIgnored()
    {
        var testText = new[]
        {
            "// line comment",
            "/* block",
            "   comment */"
        };
        var testTextWithNewlines = string.Join("\n", testText);
        var expectedOutput = "";
        RunText(testTextWithNewlines).Should().Be(expectedOutput);
    }
}
