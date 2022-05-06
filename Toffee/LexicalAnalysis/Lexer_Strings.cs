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
        while (_scanner.CurrentCharacter is not null)
        {
            var charPosition = _scanner.CurrentPosition;
            if (TryMatchEscapeSequence(out var matchedEscapeSequence))
                AppendCharConsideringLengthLimit(contentBuilder, matchedEscapeSequence, ref maxLengthExceeded, charPosition);
            else if (TryMatchCharacter(out var matchedCharacter))
                AppendCharConsideringLengthLimit(contentBuilder, matchedCharacter, ref maxLengthExceeded, charPosition);
            else
                break;
        }
        if (_scanner.CurrentCharacter is '"')
            _scanner.Advance();
        else
            EmitError(new UnexpectedEndOfText(_scanner.CurrentPosition, TokenType.LiteralString));
        return new Token(TokenType.LiteralString, contentBuilder.ToString());
    }

    private bool TryMatchEscapeSequence(out char matchedEscapeSequence)
    {
        matchedEscapeSequence = default;
        if (_scanner.CurrentCharacter is not '\\')
            return false;
        var escapeSequencePosition = _scanner.CurrentPosition;
        _scanner.Advance();

        if (_scanner.CurrentCharacter is null)
            return false;

        char EmitUnknownSequenceWarning(char specifier)
        {
            EmitWarning(new UnknownEscapeSequence(escapeSequencePosition, specifier));
            return specifier;
        }

        matchedEscapeSequence = _scanner.Advance()!.Value switch
        {
            'a'   => '\a',
            'b'   => '\b',
            'f'   => '\f',
            'n'   => '\n',
            'r'   => '\r',
            't'   => '\t',
            'v'   => '\v',
            '\\'  => '\\',
            '"'   => '"',
            '0'   => '\0',
            'x'   => MatchEscapedHexChar(escapeSequencePosition),
            var c => EmitUnknownSequenceWarning(c)
        };
        return true;
    }

    private char MatchEscapedHexChar(Position warningPosition)
    {
        const int maxHexCodeLength = 4;
        static bool IsHexDigit(char? c) => IsDigitGivenRadix(16, c);

        var digitBuffer = "";
        for (var i = 0; i < maxHexCodeLength && IsHexDigit(_scanner.CurrentCharacter); i++)
            digitBuffer += _scanner.Advance()!.Value;
        if (digitBuffer.Length == 0)
            EmitWarning(new MissingHexCharCode(warningPosition));

        var bytes = Convert.FromHexString(digitBuffer.PadLeft(maxHexCodeLength, '0'));
        // BitConverter converts data according to the endianness of the running environment.
        // However the hex code is always big-endian. Thus, the array is reversed conditionally.
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToChar(bytes);
    }

    private bool TryMatchCharacter(out char matchedCharacter)
    {
        matchedCharacter = default;
        if (_scanner.CurrentCharacter is null or '"' or '\\')
            return false;

        matchedCharacter = _scanner.Advance()!.Value;
        return true;
    }
}
