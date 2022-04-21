using System.IO;
using Xunit;

namespace Toffee.Tests;

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
            scanner.Advance();
        }

        Assert.Equal((uint)input.Length, scanner.CurrentPosition.Character);
    }
}
