using System;
using System.Linq;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Xunit;

namespace Toffee.Tests.LexicalAnalysis;

public partial class LexerTests
{
    [Trait("Category", "Strings")]
    [Theory]
    [InlineData(@"""""", "")]
    [InlineData(@""" """, " ")]
    [InlineData(@"""abcd1234""", "abcd1234")]
    [InlineData(@"""aąаαáåあア汉漢👨‍💻""", "aąаαáåあア汉漢👨‍💻")]
    [InlineData(@"""\a""", "\a")]
    [InlineData(@"""\b""", "\b")]
    [InlineData(@"""\f""", "\f")]
    [InlineData(@"""\n""", "\n")]
    [InlineData(@"""\r""", "\r")]
    [InlineData(@"""\t""", "\t")]
    [InlineData(@"""\v""", "\v")]
    [InlineData(@"""\\""", "\\")]
    [InlineData(@"""\""""", "\"")]
    [InlineData(@"""\0""", "\0")]
    [InlineData(@"""\xD""", "\xD")]
    [InlineData(@"""\x6a""", "\x6a")]
    public void StringsShouldBeRecognizedCorrectly(string input, string expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        ILexer lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.LiteralString, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
    }

    [Trait("Category", "Strings")]
    [Theory]
    [InlineData(@"""\a\b\f\n\r\t\v\\\""\0\a\b\f\n\r\t\v\\\""\0""", "\a\b\f\n\r\t\v\\\"\0\a\b\f\n\r\t\v\\\"\0")]
    [InlineData(@"""\xatest""", "\xatest")]
    [InlineData(@"""\x0123456""", "\x0123456")]
    [InlineData(@"""\xabcdefg""", "\xabcdefg")]
    [InlineData(@"""\xcactus""", "\xcactus")]
    public void BoundariesOfEscapeSequencesInStringShouldBeRecognizedCorrectly(string input, string expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        ILexer lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.LiteralString, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
    }

    [Trait("Category", "Strings")]
    [Theory]
    [InlineData(@"""abcd\efg""", "abcdefg", typeof(UnknownEscapeSequence), 5u)]
    [InlineData(@"""abcdefghijklmnopqrstuvw\xyz""", "abcdefghijklmnopqrstuvwyz", typeof(MissingHexCharCode), 24u)]
    public void IssuesInEscapeSequencesInStringsShouldBeDetectedProperly(string input, string expectedContent,
        Type expectedWarningType, uint expectedOffset)
    {
        var loggerMock = new LexerErrorHandlerMock();
        var scannerMock = new ScannerMock(input);
        ILexer lexer = new Lexer(scannerMock, loggerMock);

        Assert.Equal(TokenType.LiteralString, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        var warning = loggerMock.HandledWarnings.First(x => x.GetType() == expectedWarningType);
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), warning.Position);
    }
}
