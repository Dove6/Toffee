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

    [Trait("Category", "Identifiers")]
    [Theory]
    [InlineData("integer")]
    [InlineData("INIT")]
    [InlineData("constantinople")]
    [InlineData("ppull")]
    [InlineData("iff")]
    [InlineData("and2")]
    [InlineData("defaul")]
    public void IdentifiersBasedOnKeywordsShouldBeRecognizedCorrectly(string input)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.Identifier, lexer.CurrentToken.Type);
        Assert.Equal(input, lexer.CurrentToken.Content);
    }

    [Trait("Category", "Strings")]
    [Theory]
    [InlineData(@"""abcd\efg""", "abcdefg", typeof(UnknownEscapeSequence), 5u)]
    [InlineData(@"""abcdefghijklmnopqrstuvw\xyz""", "abcdefghijklmnopqrstuvwyz", typeof(MissingHexCharCode), 24u)]
    public void IssuesInEscapeSequencesInStringsShouldBeDetectedProperly(string input, string expectedContent, Type expectedWarningType, uint expectedOffset)
    {
        var capturedAttachments = new List<LexerWarning>();
        var logger = new Mock<ILexerErrorHandler>();
        logger.Setup(x =>
            x.Handle(Capture.In(capturedAttachments)));

        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock, logger.Object);

        Assert.Equal(TokenType.LiteralString, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        var warning = capturedAttachments.First(x => x.GetType() == expectedWarningType);
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), warning.Position);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("18446744073709551616", TokenType.LiteralInteger, 1844674407370955161ul, 19u)]
    [InlineData("10.99999999999999999999", TokenType.LiteralFloat, 10.9999999999999999999, 22u)]
    [InlineData("3.14e99999999999999999999", TokenType.LiteralFloat, double.PositiveInfinity, 24u)]
    public void NumberLiteralOverflowShouldBeDetectedProperly(string input, TokenType expectedTokenType, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(NumberLiteralTooLarge), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
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
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(lengthLimit, (lexer.CurrentError as ExceededMaxLexemeLength)!.MaxLexemeLength);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("0x", 'x', 0ul, 2u)]
    [InlineData("0xx", 'x', 0ul, 2u)]
    [InlineData("0c", 'c', 0ul, 2u)]
    [InlineData("0b", 'b', 0ul, 2u)]
    public void MissingNonDecimalDigitsShouldBeDetectedProperly(string input, char prefix, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.LiteralInteger, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(MissingNonDecimalDigits), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(prefix, (lexer.CurrentError as MissingNonDecimalDigits)!.NonDecimalPrefix);
    }

    [Trait("Category", "Numbers")]
    [Theory]
    [InlineData("0a", 'a', 0ul, 1u)]
    [InlineData("0z", 'z', 0ul, 1u)]
    [InlineData("0u", 'u', 0ul, 1u)]
    public void InvalidNonDecimalPrefixesShouldBeDetectedProperly(string input, char prefix, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.LiteralInteger, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(InvalidNonDecimalPrefix), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(prefix, (lexer.CurrentError as InvalidNonDecimalPrefix)!.NonDecimalPrefix);
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
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(expectedTokenType, (lexer.CurrentError as UnexpectedEndOfText)!.BuiltTokenType);
    }

    [Trait("Category", "Operators")]
    [Theory]
    [InlineData("`", "`", 0u)]
    [InlineData("🐲", "🐲", 0u)]
    [InlineData("\a", "\a", 0u)]
    [InlineData("?", "?", 0u)]
    [InlineData("&", "&", 0u)]
    [InlineData("|", "|", 0u)]
    public void UnknownTokensShouldBeDetectedProperly(string input, object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.Unknown, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(UnknownToken), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(input, (lexer.CurrentError as UnknownToken)!.Content);
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
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.LiteralFloat, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(MissingExponent), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
    }

    [Theory]
    [InlineData("\"string\"1234", TokenType.LiteralInteger, 1234ul)]
    [InlineData("/* comment */1234", TokenType.LiteralInteger, 1234ul)]
    [InlineData("// comment\n\"string\"", TokenType.LiteralString, "string")]
    [InlineData("?>implying", TokenType.Identifier, "implying")]
    [InlineData("1234true", TokenType.KeywordTrue, "true")]
    [InlineData("1234/* comment */", TokenType.BlockComment, " comment ")]
    [InlineData("true//this is so true", TokenType.LineComment, "this is so true")]
    [InlineData("0b11019.5", TokenType.LiteralFloat, 9.5)]
    [InlineData("500+", TokenType.OperatorPlus, "+")]
    [InlineData("112..", TokenType.OperatorDot, ".")]
    [InlineData("...", TokenType.OperatorDot, ".")]
    public void TokenInSequenceShouldHaveNoImpactOnItsSuccessor(string input, TokenType expectedTokenType, object expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        lexer.Advance();

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
    }

    [Theory]
    [InlineData("\"string\"1234", TokenType.LiteralString, "string")]
    [InlineData("/* comment */1234", TokenType.BlockComment, " comment ")]
    [InlineData("// comment\n\"string\"", TokenType.LineComment, " comment")]
    [InlineData("?>implying", TokenType.OperatorQueryGreater, "?>")]
    [InlineData("1234true", TokenType.LiteralInteger, 1234ul)]
    [InlineData("1234/* comment */", TokenType.LiteralInteger, 1234ul)]
    [InlineData("true//this is so true", TokenType.KeywordTrue, "true")]
    [InlineData("0b11019.5", TokenType.LiteralInteger, 13ul)]
    [InlineData("500+", TokenType.LiteralInteger, 500ul)]
    [InlineData("112..", TokenType.LiteralFloat, 112.0)]
    [InlineData("...", TokenType.OperatorDotDot, "..")]
    public void SupersededTokenShouldBeReturnedByAdvanceMethodCorrectly(string input, TokenType expectedTokenType, object expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        var supersededToken = lexer.Advance();

        Assert.Equal(expectedTokenType, supersededToken.Type);
        Assert.Equal(expectedContent, supersededToken.Content);
    }

    [Theory]
    [MemberData(nameof(TestSequenceEnumerable))]
    public void PositionShouldBeCalculatedCorrectly(string input, uint tokenIndex, Token expectedToken)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        for (var i = 0u; i < tokenIndex; i++)
            lexer.Advance();

        Assert.Equal(expectedToken.Type, lexer.CurrentToken.Type);
        Assert.Equal(expectedToken.StartPosition, lexer.CurrentToken.StartPosition);
        Assert.Equal(expectedToken.EndPosition, lexer.CurrentToken.EndPosition);
    }

    public static IEnumerable<object[]> TestSequenceEnumerable()
    {
        const string input = "pull std.io;\n"
            + "/* block\n"
            + "comment */\n"
            + "init const a = 5; // line comment\n"
            + "\n"
            + "print(a^2 + 3.14e0);";
        uint counter = 0;
        // ReSharper disable RedundantAssignment
        yield return new object[] { input, counter++, new Token(TokenType.KeywordPull,      "pull",             new Position(0, 1, 0),   new Position(4, 1, 4)) };
        yield return new object[] { input, counter++, new Token(TokenType.Identifier,       "std",              new Position(5, 1, 5),   new Position(8, 1, 8)) };
        yield return new object[] { input, counter++, new Token(TokenType.OperatorDot,      ".",                new Position(8, 1, 8),   new Position(9, 1, 9)) };
        yield return new object[] { input, counter++, new Token(TokenType.Identifier,       "io",               new Position(9, 1, 9),   new Position(11, 1, 11)) };
        yield return new object[] { input, counter++, new Token(TokenType.Semicolon,        ";",                new Position(11, 1, 11), new Position(12, 1, 12)) };
        yield return new object[] { input, counter++, new Token(TokenType.BlockComment,     " block\ncomment ", new Position(13, 2, 0),  new Position(32, 3, 10)) };
        yield return new object[] { input, counter++, new Token(TokenType.KeywordInit,      "init",             new Position(33, 4, 0),  new Position(37, 4, 4)) };
        yield return new object[] { input, counter++, new Token(TokenType.KeywordConst,     "const",            new Position(38, 4, 5),  new Position(43, 4, 10)) };
        yield return new object[] { input, counter++, new Token(TokenType.Identifier,       "a",                new Position(44, 4, 11), new Position(45, 4, 12)) };
        yield return new object[] { input, counter++, new Token(TokenType.OperatorEquals,   "=",                new Position(46, 4, 13), new Position(47, 4, 14)) };
        yield return new object[] { input, counter++, new Token(TokenType.LiteralInteger,   5,                  new Position(48, 4, 15), new Position(49, 4, 16)) };
        yield return new object[] { input, counter++, new Token(TokenType.Semicolon,        ";",                new Position(49, 4, 16), new Position(50, 4, 17)) };
        yield return new object[] { input, counter++, new Token(TokenType.LineComment,      " line comment",    new Position(51, 4, 18), new Position(66, 4, 33)) };
        yield return new object[] { input, counter++, new Token(TokenType.Identifier,       "print",            new Position(68, 6, 0),  new Position(73, 6, 5)) };
        yield return new object[] { input, counter++, new Token(TokenType.LeftParenthesis,  "(",                new Position(73, 6, 5),  new Position(74, 6, 6)) };
        yield return new object[] { input, counter++, new Token(TokenType.Identifier,       "a",                new Position(74, 6, 6),  new Position(75, 6, 7)) };
        yield return new object[] { input, counter++, new Token(TokenType.OperatorCaret,    "^",                new Position(75, 6, 7),  new Position(76, 6, 8)) };
        yield return new object[] { input, counter++, new Token(TokenType.LiteralInteger,   2,                  new Position(76, 6, 8),  new Position(77, 6, 9)) };
        yield return new object[] { input, counter++, new Token(TokenType.OperatorPlus,     "+",                new Position(78, 6, 10), new Position(79, 6, 11)) };
        yield return new object[] { input, counter++, new Token(TokenType.LiteralFloat,     3.14,               new Position(80, 6, 12), new Position(86, 6, 18)) };
        yield return new object[] { input, counter++, new Token(TokenType.RightParenthesis, ")",                new Position(86, 6, 18), new Position(87, 6, 19)) };
        yield return new object[] { input, counter++, new Token(TokenType.Semicolon,        ";",                new Position(87, 6, 19), new Position(88, 6, 20)) };
        // ReSharper restore RedundantAssignment
    }
}
