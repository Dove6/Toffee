using System.Text;

namespace Toffee.LexicalAnalysis;

public partial class Lexer
{
    private Token? MatchString()
    {
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
                var specifier = _scanner.CurrentCharacter.Value;
                _scanner.Advance();
                if (TryMatchEscapeSequence(specifier, out var matchedChar))
                    contentBuilder.Append(matchedChar);
                else
                    new string("Invalid escape sequence");  // TODO: error
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

    private bool TryMatchEscapeSequence(char specifier, out char result)
    {
        result = '\0';
        if (specifier is 'x')
        {
            var matchedChar = MatchEscapedHexChar();
            result = matchedChar ?? result;
            return matchedChar.HasValue;
        }
        result = specifier switch
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
            _ => specifier // TODO: error
        };
        return true;
    }

    private char? MatchEscapedHexChar()
    {
        const int maxHexCodeLength = 4;
        static bool IsHexDigit(char? c) => IsDigitGivenRadix(16, c);

        var digitBuffer = "";
        for (var i = 0; i < maxHexCodeLength && IsHexDigit(_scanner.CurrentCharacter); i++)
        {
            digitBuffer += _scanner.CurrentCharacter!.Value;
            _scanner.Advance();
        }
        if (digitBuffer.Length == 0)
            new string("Missing hex code"); // TODO: error

        var bytes = Convert.FromHexString(digitBuffer.PadLeft(maxHexCodeLength, '0'));
        // BitConverter converts data according to the endianness of the running environment.
        // However the hex code is always big-endian. Thus, the array is reversed conditionally.
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToChar(bytes);
    }
}
