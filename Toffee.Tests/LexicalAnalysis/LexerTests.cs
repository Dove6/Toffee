using Toffee.LexicalAnalysis;
using Xunit;

namespace Toffee.Tests.LexicalAnalysis;

public class LexerTests
{
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
}
