using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Toffee.LexicalAnalysis;
using Toffee.Logging;
using Toffee.Scanning;
using Xunit;

namespace Toffee.Tests.LexicalAnalysis;

public class LexerTests
{
    [Trait("Category", "Operators")]
    [Theory]
    [InlineData(".", TokenType.OperatorDot)]
    [InlineData("?.", TokenType.OperatorQueryDot)]
    [InlineData("^", TokenType.OperatorCaret)]
    [InlineData("+", TokenType.OperatorPlus)]
    [InlineData("-", TokenType.OperatorMinus)]
    [InlineData("!", TokenType.OperatorBang)]
    [InlineData("*", TokenType.OperatorAsterisk)]
    [InlineData("/", TokenType.OperatorSlash)]
    [InlineData("%", TokenType.OperatorPercent)]
    [InlineData("..", TokenType.OperatorDotDot)]
    [InlineData("<", TokenType.OperatorLess)]
    [InlineData("<=", TokenType.OperatorLessEquals)]
    [InlineData(">", TokenType.OperatorGreater)]
    [InlineData(">=", TokenType.OperatorGreaterEquals)]
    [InlineData("==", TokenType.OperatorEqualsEquals)]
    [InlineData("!=", TokenType.OperatorBangEquals)]
    [InlineData("&&", TokenType.OperatorAndAnd)]
    [InlineData("||", TokenType.OperatorOrOr)]
    [InlineData("??", TokenType.OperatorQueryQuery)]
    [InlineData("?>", TokenType.OperatorQueryGreater)]
    [InlineData("=", TokenType.OperatorEquals)]
    [InlineData("+=", TokenType.OperatorPlusEquals)]
    [InlineData("-=", TokenType.OperatorMinusEquals)]
    [InlineData("*=", TokenType.OperatorAsteriskEquals)]
    [InlineData("/=", TokenType.OperatorSlashEquals)]
    [InlineData("%=", TokenType.OperatorPercentEquals)]
    [InlineData("(", TokenType.LeftParenthesis)]
    [InlineData(")", TokenType.RightParenthesis)]
    [InlineData("{", TokenType.LeftBrace)]
    [InlineData("}", TokenType.RightBrace)]
    [InlineData(",", TokenType.Comma)]
    [InlineData(":", TokenType.Colon)]
    [InlineData(";", TokenType.Semicolon)]
    public void OperatorsShouldBeRecognizedCorrectly(string input, TokenType expectedTokenType)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
    }

    [Trait("Category", "Comments")]
    [Theory]
    [InlineData("//", TokenType.LineComment)]
    [InlineData("/*", TokenType.BlockComment)]
    [InlineData("/**/", TokenType.BlockComment)]
    public void CommentsShouldBeRecognizedCorrectly(string input, TokenType expectedTokenType)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
    }

    [Trait("Category", "Comments")]
    [Theory]
    [InlineData("//", false, "")]
    [InlineData("// ", false, " ")]
    [InlineData("// example content", false, " example content")]
    [InlineData("// example\nmultiline\ncontent", false, " example")]
    [InlineData("///**/", false, "/**/")]
    [InlineData("/*", true, "")]
    [InlineData("/**/", true, "")]
    [InlineData("/* */", true, " ")]
    [InlineData("/* example content */", true, " example content ")]
    [InlineData("/* example\nmultiline\ncontent */", true, " example\nmultiline\ncontent ")]
    [InlineData("/*///* /**/", true, "///* /*")]
    public void ContentOfCommentsShouldBePreservedProperly(string input, bool isBlock, string expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(isBlock ? TokenType.BlockComment : TokenType.LineComment, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
    }

    [Fact]
    public void EmptyInputShouldResultInEtxToken()
    {
        var scannerMock = new ScannerMock("");
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.EndOfText, lexer.CurrentToken.Type);

        lexer.Advance();
        Assert.Equal(TokenType.EndOfText, lexer.CurrentToken.Type);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(" \t\v")]
    [InlineData("\n")]
    [InlineData("\u2029")]
    public void WhiteSpacesShouldBeSkipped(string input)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.EndOfText, lexer.CurrentToken.Type);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("1", 1L)]
    [InlineData("0", 0L)]
    [InlineData("9223372036854775807", 9223372036854775807L)]
    [InlineData("0000001", 1L)]
    [InlineData("01", 1L)]
    [InlineData("0x1", 1L)]
    [InlineData("0x001", 1L)]
    [InlineData("0xabCD", 43981L)]
    [InlineData("0c1", 1L)]
    [InlineData("0c001", 1L)]
    [InlineData("0c741", 481L)]
    [InlineData("0b1", 1L)]
    [InlineData("0b0001", 1L)]
    [InlineData("0b1011", 11L)]
    public void IntegersShouldBeRecognizedCorrectly(string input, long expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.LiteralInteger, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
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
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.LiteralFloat, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
    }

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
        var lexer = new Lexer(scannerMock);

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
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.LiteralString, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
    }

    [Trait("Category", "Keywords")]
    [Theory]
    [InlineData("int", TokenType.KeywordInt)]
    [InlineData("float", TokenType.KeywordFloat)]
    [InlineData("string", TokenType.KeywordString)]
    [InlineData("bool", TokenType.KeywordBool)]
    [InlineData("function", TokenType.KeywordFunction)]
    [InlineData("null", TokenType.KeywordNull)]
    [InlineData("init", TokenType.KeywordInit)]
    [InlineData("const", TokenType.KeywordConst)]
    [InlineData("pull", TokenType.KeywordPull)]
    [InlineData("if", TokenType.KeywordIf)]
    [InlineData("elif", TokenType.KeywordElif)]
    [InlineData("else", TokenType.KeywordElse)]
    [InlineData("while", TokenType.KeywordWhile)]
    [InlineData("for", TokenType.KeywordFor)]
    [InlineData("break", TokenType.KeywordBreak)]
    [InlineData("break_if", TokenType.KeywordBreakIf)]
    [InlineData("functi", TokenType.KeywordFuncti)]
    [InlineData("return", TokenType.KeywordReturn)]
    [InlineData("match", TokenType.KeywordMatch)]
    [InlineData("and", TokenType.KeywordAnd)]
    [InlineData("or", TokenType.KeywordOr)]
    [InlineData("is", TokenType.KeywordIs)]
    [InlineData("not", TokenType.KeywordNot)]
    [InlineData("default", TokenType.KeywordDefault)]
    [InlineData("false", TokenType.KeywordFalse)]
    [InlineData("true", TokenType.KeywordTrue)]
    public void KeywordsShouldBeRecognizedCorrectly(string input, TokenType expectedTokenType)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
    }

    [Trait("Category", "Strings")]
    [Theory]
    [InlineData(@"""abcd\efg""", "abcdefg", typeof(UnknownEscapeSequence), 5u)]
    [InlineData(@"""abcdefghijklmnopqrstuvw\xyz""", "abcdefghijklmnopqrstuvwyz", typeof(MissingHexCharCode), 24u)]
    public void IssuesInEscapeSequencesInStringsShouldBeDetectedProperly(string input, string expectedContent, Type expectedWarningType, uint expectedOffset)
    {
        var capturedAttachments = new List<object>();
        var logger = new Mock<Logger>("");
        logger.Setup(x =>
            x.Log(LogLevel.Warning, It.IsAny<Position>(), It.IsAny<string>(), Capture.In(capturedAttachments)));

        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock, logger.Object);

        Assert.Equal(TokenType.LiteralString, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        var warning = capturedAttachments.First(x => x.GetType() == expectedWarningType) as LexerWarning;
        Assert.Equal(expectedOffset, warning!.Offset);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("9223372036854775808", TokenType.LiteralInteger, 922337203685477580L, 18u)]
    [InlineData("10.9999999999999999999", TokenType.LiteralFloat, 10.999999999999999999, 21u)]
    [InlineData("3.14e9999999999999999999", TokenType.LiteralFloat, double.PositiveInfinity, 23u)]
    public void NumberLiteralOverflowShouldBeDetectedProperly(string input, TokenType expectedTokenType, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(NumberLiteralTooLarge), lexer.CurrentError?.GetType());
        Assert.Equal(expectedOffset, lexer.CurrentError!.Offset);
    }

    [Trait("Category", "Strings")]
    [Trait("Category", "Comments")]
    [Trait("Category", "Identifiers")]
    [Theory]
    [InlineData(@"""abcdefg""", 3, TokenType.LiteralString, "abc", 4u)]
    [InlineData("// abcdefg", 4, TokenType.LineComment, " abc", 6u)]
    [InlineData("/* abcdefg */", 4, TokenType.BlockComment, " abc", 6u)]
    [InlineData("abcdefg", 3, TokenType.Identifier, "abc", 3u)]
    public void ExcessLexemeLengthShouldBeDetectedProperly(string input, int lengthLimit, TokenType expectedTokenType, string expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock, maxLexemeLength: lengthLimit);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(ExceededMaxLexemeLength), lexer.CurrentError?.GetType());
        Assert.Equal(expectedOffset, lexer.CurrentError!.Offset);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("0x", TokenType.LiteralInteger, 0L, 2u)]
    [InlineData("0c", TokenType.LiteralInteger, 0L, 2u)]
    [InlineData("0b", TokenType.LiteralInteger, 0L, 2u)]
    public void MissingNonDecimalDigitsShouldBeDetectedProperly(string input, TokenType expectedTokenType, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(MissingNonDecimalDigits), lexer.CurrentError?.GetType());
        Assert.Equal(expectedOffset, lexer.CurrentError!.Offset);
    }

    [Trait("Category", "Strings")]
    [Trait("Category", "Comments")]
    [Theory]
    [InlineData(@"""nerotaruk", TokenType.LiteralString, "nerotaruk", 10u)]
    [InlineData("/* zaq1@WSX", TokenType.BlockComment, " zaq1@WSX", 11u)]
    public void UnexpectedEndOfTextShouldBeDetectedProperly(string input, TokenType expectedTokenType, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(UnexpectedEndOfText), lexer.CurrentError?.GetType());
        Assert.Equal(expectedOffset, lexer.CurrentError!.Offset);
    }
    // TODO: ETX for "promising" operators

    [Trait("Category", "Operators")]
    [Theory]
    [InlineData("`", '`', 0u)]
    [InlineData("🐲", '\uD83D', 0u)]
    [InlineData("\a", '\a', 0u)]
    public void UnknownTokensShouldBeDetectedProperly(string input, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.Unknown, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(UnknownToken), lexer.CurrentError?.GetType());
        Assert.Equal(expectedOffset, lexer.CurrentError!.Offset);
    }
}
