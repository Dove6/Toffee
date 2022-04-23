using System.Text;

namespace Toffee.LexicalAnalysis;

public partial class Lexer
{
    private Token? MatchString()
    {
        static bool IsHexDigit(char? c) => IsDigitGivenRadix(16, c);

        // TODO: limit length
        if (_scanner.CurrentCharacter is not '"')
            return null;
        _scanner.Advance();
        var contentBuilder = new StringBuilder();
        var escaping = false;
        while (_scanner.CurrentCharacter is not null)
        {
            if (escaping)
            {
                escaping = false;
                if (_scanner.CurrentCharacter is 'x')
                {
                    // TODO: support 2-byte chars
                    // TODO: use BitConverter
                    _scanner.Advance();
                    if (!IsHexDigit(_scanner.CurrentCharacter))
                        new string("Missing hex byte specification"); // TODO: error
                    var escapedByte = (byte)CharToDigit(_scanner.CurrentCharacter.Value);
                    _scanner.Advance();
                    if (IsHexDigit(_scanner.CurrentCharacter))
                    {
                        escapedByte = (byte)(16 * escapedByte + CharToDigit(_scanner.CurrentCharacter.Value));
                        _scanner.Advance();
                    }
                    contentBuilder.Append((char)escapedByte);
                }
                else
                {
                    contentBuilder.Append(_scanner.CurrentCharacter switch
                    {
                        'a' => '\a',
                        'b' => '\b',
                        'f' => '\f',
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        'v' => '\v',
                        '\\' => '\\',
                        '"' => '"',
                        '0' => '\0',
                        _ => _scanner.CurrentCharacter.Value // TODO: error
                    });
                    _scanner.Advance();
                }
            }
            else
            {
                if (_scanner.CurrentCharacter is '"')
                    break;
                if (_scanner.CurrentCharacter is '\\')
                    escaping = true;
                else
                    contentBuilder.Append(_scanner.CurrentCharacter.Value);
                _scanner.Advance();
            }
        }
        if (_scanner.CurrentCharacter is '"')
            _scanner.Advance();
        else
            new string("Unexpected ETX"); // TODO: error
        return new Token(TokenType.LiteralString, contentBuilder.ToString());
    }
}
