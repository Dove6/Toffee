using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Xunit;

namespace Toffee.Tests.LexicalAnalysis;

public partial class LexerTests
{
    [Trait("Category", "Operators")]
    [Theory]
    [InlineData(".", TokenType.OperatorDot)]
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
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Comments")]
    [Theory]
    [InlineData("//", TokenType.LineComment, false)]
    [InlineData("/*", TokenType.BlockComment, true)]
    [InlineData("/**/", TokenType.BlockComment, false)]
    public void CommentsShouldBeRecognizedCorrectly(string input, TokenType expectedTokenType, bool shouldProduceError)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);

        Assert.False(shouldProduceError ^ errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Comments")]
    [Theory]
    [InlineData("//", false, "", false)]
    [InlineData("// ", false, " ", false)]
    [InlineData("// example content", false, " example content", false)]
    [InlineData("// example\nmultiline\ncontent", false, " example", false)]
    [InlineData("///**/", false, "/**/", false)]
    [InlineData("/*", true, "", true)]
    [InlineData("/**/", true, "", false)]
    [InlineData("/* */", true, " ", false)]
    [InlineData("/* example content */", true, " example content ", false)]
    [InlineData("/* example\nmultiline\ncontent */", true, " example\nmultiline\ncontent ", false)]
    [InlineData("/*///* /**/", true, "///* /*", false)]
    public void ContentOfCommentsShouldBePreservedProperly(string input, bool isBlock, string expectedContent, bool shouldProduceError)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(isBlock ? TokenType.BlockComment : TokenType.LineComment, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);

        Assert.False(shouldProduceError ^ errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
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
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.Unknown, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(UnknownToken), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(input, (lexer.CurrentError as UnknownToken)!.Content);

        Assert.False(errorHandlerMock.HadWarnings);
    }
}
