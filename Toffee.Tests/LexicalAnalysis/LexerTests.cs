using System.Collections.Generic;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;
using Xunit;

namespace Toffee.Tests.LexicalAnalysis;

public partial class LexerTests
{
    [Fact]
    public void EmptyInputShouldResultInEtxToken()
    {
        var scannerMock = new ScannerMock("");
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.EndOfText, lexer.CurrentToken.Type);

        lexer.Advance();
        Assert.Equal(TokenType.EndOfText, lexer.CurrentToken.Type);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(" \t\v")]
    [InlineData("\n")]
    [InlineData("\u2029")]
    public void WhiteSpacesShouldBeSkipped(string input)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(TokenType.EndOfText, lexer.CurrentToken.Type);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Strings")]
    [Trait("Category", "Comments")]
    [Trait("Category", "Identifiers")]
    [Theory]
    [InlineData(@"""abcdefg""", 3, TokenType.LiteralString, "abc", 4u)]
    [InlineData("// abcdefg", 4, TokenType.LineComment, " abc", 6u)]
    [InlineData("/* abcdefg */", 4, TokenType.BlockComment, " abc", 6u)]
    [InlineData("abcdefg", 3, TokenType.Identifier, "abc", 3u)]
    public void ExcessLexemeLengthShouldBeDetectedProperly(string input, int lengthLimit, TokenType expectedTokenType,
        string expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock, lengthLimit);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(ExceededMaxLexemeLength), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(lengthLimit, (lexer.CurrentError as ExceededMaxLexemeLength)!.MaxLexemeLength);

        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Trait("Category", "Strings")]
    [Trait("Category", "Comments")]
    [Theory]
    [InlineData(@"""nerotaruk", TokenType.LiteralString, "nerotaruk", 10u)]
    [InlineData("/* zaq1@WSX", TokenType.BlockComment, " zaq1@WSX", 11u)]
    public void UnexpectedEndOfTextShouldBeDetectedProperly(string input, TokenType expectedTokenType,
        object expectedContent, uint expectedOffset)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);
        Assert.Equal(typeof(UnexpectedEndOfText), lexer.CurrentError?.GetType());
        Assert.Equal(new Position(expectedOffset, 1, expectedOffset), lexer.CurrentError!.Position);
        Assert.Equal(expectedTokenType, (lexer.CurrentError as UnexpectedEndOfText)!.BuiltTokenType);

        Assert.False(errorHandlerMock.HadWarnings);
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
    public void TokenInSequenceShouldHaveNoImpactOnItsSuccessor(string input, TokenType expectedTokenType,
        object expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        lexer.Advance();

        Assert.Equal(expectedTokenType, lexer.CurrentToken.Type);
        Assert.Equal(expectedContent, lexer.CurrentToken.Content);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
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
    public void SupersededTokenShouldBeReturnedByAdvanceMethodCorrectly(string input, TokenType expectedTokenType,
        object expectedContent)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        var supersededToken = lexer.Advance();

        Assert.Equal(expectedTokenType, supersededToken.Type);
        Assert.Equal(expectedContent, supersededToken.Content);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
    }

    [Theory]
    [MemberData(nameof(TestSequenceEnumerable))]
    public void PositionShouldBeCalculatedCorrectly(string input, uint tokenIndex, Token expectedToken)
    {
        var scannerMock = new ScannerMock(input);
        var errorHandlerMock = new LexerErrorHandlerMock();
        ILexer lexer = new Lexer(scannerMock, errorHandlerMock);

        for (var i = 0u; i < tokenIndex; i++)
            lexer.Advance();

        Assert.Equal(expectedToken.Type, lexer.CurrentToken.Type);
        Assert.Equal(expectedToken.StartPosition, lexer.CurrentToken.StartPosition);
        Assert.Equal(expectedToken.EndPosition, lexer.CurrentToken.EndPosition);

        Assert.False(errorHandlerMock.HadErrors);
        Assert.False(errorHandlerMock.HadWarnings);
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
