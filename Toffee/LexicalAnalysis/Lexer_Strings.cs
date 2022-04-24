using System.Text;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer
{
    private Token? MatchString()
    {
        // TODO: OoM exception
        if (_scanner.CurrentCharacter is not '"')
            return null;
        _scanner.Advance();
        var contentBuilder = new StringBuilder();
        var maxLengthExceeded = false;
        var escaping = false;
        while (_scanner.CurrentCharacter is not null)
        {
            if (escaping)
            {
                escaping = false;
                var escapeSequenceOffset = CurrentOffset - 1;
                var specifier = _scanner.CurrentCharacter.Value;
                _scanner.Advance();
                var matchedEscapeSequence = MatchEscapeSequence(specifier);
                AppendCharConsideringLengthLimit(contentBuilder, matchedEscapeSequence, ref maxLengthExceeded, escapeSequenceOffset);
            }
            else
            {
                if (_scanner.CurrentCharacter is '"')
                    break;
                if (_scanner.CurrentCharacter is '\\')
                    escaping = true;
                else
                    AppendCharConsideringLengthLimit(contentBuilder, _scanner.CurrentCharacter, ref maxLengthExceeded);
                _scanner.Advance();
            }
        }
        if (_scanner.CurrentCharacter is '"')
            _scanner.Advance();
        else
            EmitError(new UnexpectedEndOfText(CurrentOffset));
        return new Token(TokenType.LiteralString, contentBuilder.ToString());
    }

    private char? MatchEscapeSequence(char specifier)
    {
        char EmitUnknownSequenceWarning()
        {
            EmitWarning(new UnknownEscapeSequence(specifier, CurrentOffset - 1));
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
            'x'  => MatchEscapedHexChar(),
            _    => EmitUnknownSequenceWarning()
        };
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
            EmitWarning(new HexCharCodeMissing(CurrentOffset));

        var bytes = Convert.FromHexString(digitBuffer.PadLeft(maxHexCodeLength, '0'));
        // BitConverter converts data according to the endianness of the running environment.
        // However the hex code is always big-endian. Thus, the array is reversed conditionally.
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToChar(bytes);
    }
}
