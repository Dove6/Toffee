using Toffee.LexicalAnalysis;
using Xunit;

namespace Toffee.Tests.LexicalAnalysis;

public class LexerTests
{
    // TODO: negative tests

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
    public void WhiteSpacesShouldBeSkipped(string input)
    {
        var scannerMock = new ScannerMock(input);
        var lexer = new Lexer(scannerMock);

        Assert.Equal(TokenType.EndOfText, lexer.CurrentToken.Type);
    }

    [Theory]
    [InlineData("1", 1L)]
    [InlineData("0", 0L)]
    [InlineData("9223372036854775807", 9223372036854775807L)]
    [InlineData("0000001", 1L)]
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
}
