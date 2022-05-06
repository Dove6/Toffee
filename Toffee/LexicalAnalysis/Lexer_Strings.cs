using System.Text;
using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer
{
    private Token? MatchString()
    {
        if (_scanner.CurrentCharacter is not '"')
            return null;
        _scanner.Advance();
        var contentBuilder = new StringBuilder();
        var maxLengthExceeded = false;
        var escaping = false;
        var escapeSequencePosition = _scanner.CurrentPosition;
        while (_scanner.CurrentCharacter is not null)
        {
            if (escaping)
            {
                escaping = false;
                var specifier = _scanner.CurrentCharacter.Value;
                _scanner.Advance();
                var matchedEscapeSequence = MatchEscapeSequence(specifier, escapeSequencePosition);
                AppendCharConsideringLengthLimit(contentBuilder, matchedEscapeSequence, ref maxLengthExceeded, escapeSequencePosition);
            }
            else
            {
                if (_scanner.CurrentCharacter is '"')
                    break;
                if (_scanner.CurrentCharacter is '\\')
                {
                    escaping = true;
                    escapeSequencePosition = _scanner.CurrentPosition;
                }
                else
                    AppendCharConsideringLengthLimit(contentBuilder, _scanner.CurrentCharacter, ref maxLengthExceeded);
                _scanner.Advance();
            }
        }
        if (_scanner.CurrentCharacter is '"')
            _scanner.Advance();
        else
            EmitError(new UnexpectedEndOfText(_scanner.CurrentPosition, TokenType.LiteralString));
        return new Token(TokenType.LiteralString, contentBuilder.ToString());
    }

    private char? MatchEscapeSequence(char specifier, Position warningPosition)
    {
        char EmitUnknownSequenceWarning()
        {
            EmitWarning(new UnknownEscapeSequence(warningPosition, specifier));
            return specifier;
        }

        return specifier switch
        {
            'a'  => '\a',
            'b'  => '\b',
            'f'  => '\f',
            'n'  => '\n',
            'r'  => '\r',
            't'  => '\t',
            'v'  => '\v',
            '\\' => '\\',
            '"'  => '"',
            '0'  => '\0',
            'x'  => MatchEscapedHexChar(warningPosition),
            _    => EmitUnknownSequenceWarning()
        };
    }

    private char? MatchEscapedHexChar(Position warningPosition)
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
            EmitWarning(new MissingHexCharCode(warningPosition));

        var bytes = Convert.FromHexString(digitBuffer.PadLeft(maxHexCodeLength, '0'));
        // BitConverter converts data according to the endianness of the running environment.
        // However the hex code is always big-endian. Thus, the array is reversed conditionally.
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToChar(bytes);
    }
}
