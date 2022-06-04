using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Xunit;

namespace Toffee.Tests.LexicalAnalysis;

public partial class LexerTests
{
    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("1", 1ul)]
    [InlineData("0", 0ul)]
    [InlineData("9223372036854775807", 9223372036854775807ul)]
    [InlineData("9223372036854775808", 9223372036854775808ul)]
    [InlineData("18446744073709551615", 18446744073709551615ul)]
    [InlineData("0000001", 1ul)]
    [InlineData("01", 1ul)]
    [InlineData("0x1", 1ul)]
    [InlineData("0x001", 1ul)]
    [InlineData("0xabCD", 43981ul)]
    [InlineData("0c1", 1ul)]
    [InlineData("0c001", 1ul)]
    [InlineData("0c741", 481ul)]
    [InlineData("0b1", 1ul)]
    [InlineData("0b0001", 1ul)]
    [InlineData("0b1011", 11ul)]
    public void IntegersShouldBeRecognizedCorrectly(string input, ulong expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.LiteralInteger, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("1.", 1.0)]
    [InlineData("0.", 0.0)]
    [InlineData("1.2345", 1.2345)]
    [InlineData("000000.1", 0.1)]
    [InlineData("1.7976931348623157E+308", 1.7976931348623157e308)]
    [InlineData("2.2e1", 22.0)]
    [InlineData("2.2e-1", 0.22)]
    [InlineData("2.2e+1", 22.0)]
    [InlineData("002.e1", 20.0)]
    [InlineData("0.0e0", 0.0)]
    [InlineData("2.E-0", 2.0)]
    public void FloatsShouldBeRecognizedCorrectly(string input, double expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.LiteralFloat, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("18446744073709551616", TokenType.LiteralInteger, 1844674407370955161ul, 19u)]
    [InlineData("10.99999999999999999999", TokenType.LiteralFloat, 10.9999999999999999999, 22u)]
    [InlineData("3.14e99999999999999999999", TokenType.LiteralFloat, double.PositiveInfinity, 24u)]
    public void NumberLiteralOverflowShouldBeDetectedProperly(string input, TokenType expectedTokenType,
        object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(NumberLiteralTooLarge), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("0x", 'x', 0ul, 2u)]
    [InlineData("0xx", 'x', 0ul, 2u)]
    [InlineData("0c", 'c', 0ul, 2u)]
    [InlineData("0b", 'b', 0ul, 2u)]
    public void MissingNonDecimalDigitsShouldBeDetectedProperly(string input, char prefix, object expectedContent,
        uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.LiteralInteger, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(MissingNonDecimalDigits), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(prefix, (lexer.CurrentError as MissingNonDecimalDigits)!.NonDecimalPrefix);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("0a", 'a', 0ul, 1u)]
    [InlineData("0z", 'z', 0ul, 1u)]
    [InlineData("0u", 'u', 0ul, 1u)]
    public void InvalidNonDecimalPrefixesShouldBeDetectedProperly(string input, char prefix, object expectedContent,
        uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.LiteralInteger, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(InvalidNonDecimalPrefix), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(prefix, (lexer.CurrentError as InvalidNonDecimalPrefix)!.NonDecimalPrefix);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("12.e", 12.0, 4u)]
    [InlineData("1234.5678e+", 1234.5678, 11u)]
    [InlineData("0.5e--", 0.5, 5u)]
    [InlineData("789.ee", 789.0, 5u)]
    public void MissingExponentShouldBeDetectedProperly(string input, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.LiteralFloat, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(MissingExponent), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
