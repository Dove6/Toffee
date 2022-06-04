using Toffee.LexicalAnalysis;
using Xunit;

namespace Toffee.Tests.LexicalAnalysis;

public partial class LexerTests
{
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
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
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
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.Identifier, lexer.CurrentToken.Type);
        Assert.Equal(input, lexer.CurrentToken.Content);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }
}
