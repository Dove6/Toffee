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
    public void ExtendingCapturedScopeShouldNotBePossible()
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
    public void PatternMatchingShouldWork()
    {
        var testText = Join(new[]
        {
            "init const matchingExample = functi(var) {",
            "    match(var) {",
            "        is null:        \"null value\";",
            "        == 5:           \"exactly five\";",
            "        is int and > 8: \"integer greater than 8\";",
            "        default:        \"no match\";",
            "    }",
            "};",
            "print(matchingExample(5));",
            "print(matchingExample(10));",
            "print(matchingExample(null));",
            "print(matchingExample(\"5\"));"
        });
        var expectedOutput = Join(new[]
        {
            "exactly five",
            "integer greater than 8",
            "null value",
            "no match"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void RecursionShouldWork()
    {
        var testText = Join(new[]
        {
            "init const factorial = functi(n) {",
            "    if (n > 1)",
            "        n * factorial(n - 1)",
            "    else",
            "        1",
            "};",
            "print(factorial(1));",
            "print(factorial(5));",
        });
        var expectedOutput = Join(new[]
        {
            "1",
            "120"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void ReturnShouldExitOnlyCurrentFunction()
    {
        var testText = Join(new[]
        {
            "init c = functi() {",
            "    init inner = (functi() {",
            "        return 5;",
            "        6",
            "    })();",
            "    return inner + 2;",
            "    10",
            "};",
            "print(c());"
        });
        var expectedOutput = Join(new[]
        {
            "7"
        });
        var output = RunText(testText);
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void BreakShouldExitOnlyCurrentLoop()
    {
        var testText = Join(new[]
        {
            "for(i, 2) {",
            "    init inner;",
            "    for(j, 9:-1:-1) {",
            "        inner = j;",
            "        break_if(j == 2);",
            "    };",
            "    print(i, inner);",
            "};"
        });
        var expectedOutput = Join(new[]
        {
            "0",
            "2",
            "1",
            "2"
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
