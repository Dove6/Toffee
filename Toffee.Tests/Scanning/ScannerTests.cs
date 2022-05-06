using System.IO;
using System.Linq;
using Toffee.Scanning;
using Xunit;

namespace Toffee.Tests.Scanning;

public class ScannerTests
{
    [Fact]
    public void EmptyInputShouldResultInNoOutput()
    {
        const string input = "";
        var scanner = new Scanner(new StringReader(input));

        Assert.Null(scanner.CurrentCharacter);

        scanner.Advance();
        Assert.Null(scanner.CurrentCharacter);
    }

    [Fact]
    public void PositionShouldNotBeIncrementedAfterEtx()
    {
        const string input = "";
        var scanner = new Scanner(new StringReader(input));
        var previousPosition = scanner.CurrentPosition;

        scanner.Advance();
        Assert.Equal(previousPosition, scanner.CurrentPosition);
    }

    [Theory]
    [InlineData("\n\r")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    [InlineData("\x1e")]
    public void NewLineSequencesShouldBeRecognizedCorrectly(string input)
    {
        const char outputNewLineSymbol = '\n';
        var scanner = new Scanner(new StringReader(input));

        Assert.Equal(outputNewLineSymbol, scanner.CurrentCharacter);

        scanner.Advance();
        Assert.Null(scanner.CurrentCharacter);
        Assert.Equal((uint)input.Length, scanner.CurrentPosition.Character);
    }

    [Fact]
    public void NonNewLineCharactersShouldBeLeftIntact()
    {
        const string input = "abcd1234";
        var scanner = new Scanner(new StringReader(input));

        foreach (var character in input)
        {
            Assert.Equal(character, scanner.CurrentCharacter);
            scanner.Advance();
        }

        Assert.Null(scanner.CurrentCharacter);
    }

    [Fact]
    public void NonNewLineCharactersShouldIncrementPositionProperly()
    {
        const string input = "abcd1234";
        var scanner = new Scanner(new StringReader(input));

        for (var i = 0u; i < input.Length; i++)
        {
            Assert.Equal(i, scanner.CurrentPosition.Character);
            Assert.Equal(1u, scanner.CurrentPosition.Line);
            Assert.Equal(i, scanner.CurrentPosition.Column);
            scanner.Advance();
        }

        Assert.Equal((uint)input.Length, scanner.CurrentPosition.Character);
        Assert.Equal(1u, scanner.CurrentPosition.Line);
        Assert.Equal((uint)input.Length, scanner.CurrentPosition.Column);
    }

    [Fact]
    public void NewLineCharactersShouldIncrementPositionProperly()
    {
        const string input = "\n\n\n";
        var scanner = new Scanner(new StringReader(input));

        for (var i = 0u; i < input.Length; i++)
        {
            Assert.Equal(i, scanner.CurrentPosition.Character);
            Assert.Equal(i + 1, scanner.CurrentPosition.Line);
            Assert.Equal(0u, scanner.CurrentPosition.Column);
            scanner.Advance();
        }

        Assert.Equal((uint)input.Length, scanner.CurrentPosition.Character);
        Assert.Equal((uint)input.Length + 1, scanner.CurrentPosition.Line);
        Assert.Equal(0u, scanner.CurrentPosition.Column);
    }

    [Fact]
    public void DifferentNewLineCharactersShouldIncrementPositionProperly()
    {
        const string input = "\n" + "\n\r" + "\r" + "\r\n" + "\x1e" + "\x1e";
        var increments = new[]{ 1u, 2u, 1u, 2u, 1u, 1u };
        var scanner = new Scanner(new StringReader(input));

        for (var i = 0u; i < increments.Length; i++)
        {
            Assert.Equal(increments.Take((int)i).Sum(x => x), scanner.CurrentPosition.Character);
            Assert.Equal(i + 1, scanner.CurrentPosition.Line);
            Assert.Equal(0u, scanner.CurrentPosition.Column);
            scanner.Advance();
        }

        Assert.Equal((uint)increments.Sum(x => x), scanner.CurrentPosition.Character);
        Assert.Equal((uint)increments.Length + 1, scanner.CurrentPosition.Line);
        Assert.Equal(0u, scanner.CurrentPosition.Column);
    }

    [Fact]
    public void SupersededCharactersShouldBeReturnedByAdvanceMethodCorrectly()
    {
        const string input = "abcd1234";
        var scanner = new Scanner(new StringReader(input));

        foreach (var character in input)
            Assert.Equal(character, scanner.Advance());

        Assert.Null(scanner.Advance());
    }
}
