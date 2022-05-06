using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer
{
    private Token? MatchNumber()
    {
        // TODO: handle <-9223372036854775808 and >9223372036854775808 "literals" in parser
        if (!IsDigitGivenRadix(10, _scanner.CurrentCharacter))
            return null;

        var initialD = _scanner.CurrentCharacter!.Value;
        _scanner.Advance();
        if (_scanner.CurrentCharacter is null || !char.IsLetter(_scanner.CurrentCharacter!.Value))
            return ContinueMatchingDecimalNumber(initialD);
        var prefix = _scanner.CurrentCharacter.Value;
        var prefixPosition = _scanner.CurrentPosition;
        _scanner.Advance();
        return ContinueMatchingNonDecimalInteger(prefix, prefixPosition);
    }

    private Token ContinueMatchingDecimalNumber(char initialDigit)
    {
        static bool IsDigit(char? c) => IsDigitGivenRadix(10, c);
        void AppendDigitConsideringOverflow(ref ulong buffer, char digit, ref bool overflowOccurred, Position? errorPosition = null) =>
            AppendDigitConsideringOverflowGivenRadix(10, ref buffer, digit, ref overflowOccurred, errorPosition);

        var overflowOccurred = false;

        var integralPart = (ulong)CharToDigit(initialDigit);
        while (IsDigit(_scanner.CurrentCharacter))
        {
            AppendDigitConsideringOverflow(ref integralPart, _scanner.CurrentCharacter!.Value, ref overflowOccurred);
            _scanner.Advance();
        }
        if (_scanner.CurrentCharacter is not '.')  // no fractional part
            return new Token(TokenType.LiteralInteger, integralPart);

        _scanner.Advance();
        var fractionalPart = 0ul;
        var fractionalPartLength = 0;
        while (IsDigit(_scanner.CurrentCharacter))
        {
            AppendDigitConsideringOverflow(ref fractionalPart, _scanner.CurrentCharacter!.Value, ref overflowOccurred);
            _scanner.Advance();
            if (!overflowOccurred)
                fractionalPartLength++;
        }
        if (_scanner.CurrentCharacter is not ('e' or 'E')) // no exponential part
        {
            var joinedNumber = integralPart + fractionalPart / Math.Pow(10, fractionalPartLength);
            return new Token(TokenType.LiteralFloat, joinedNumber);
        }

        _scanner.Advance();
        var exponentialPart = 0ul;
        var exponentSign = _scanner.CurrentCharacter is '-' ? -1 : 1;
        if (_scanner.CurrentCharacter is '-' or '+')
            _scanner.Advance();
        if (!IsDigit(_scanner.CurrentCharacter))
            EmitError(new MissingExponent(_scanner.CurrentPosition));
        while (IsDigit(_scanner.CurrentCharacter))
        {
            AppendDigitConsideringOverflow(ref exponentialPart, _scanner.CurrentCharacter!.Value, ref overflowOccurred);
            _scanner.Advance();
        }
        var exponentiatedNumber = integralPart * Math.Pow(10, exponentSign * (double)exponentialPart)
            + fractionalPart * Math.Pow(10, exponentSign * (double)exponentialPart - fractionalPartLength);
        return new Token(TokenType.LiteralFloat, exponentiatedNumber);
    }

    private Token ContinueMatchingNonDecimalInteger(char prefix, Position prefixPosition)
    {
        var (radix, isPrefixValid) = prefix switch
        {
            'x' => (16, true),
            'c' => (8, true),
            'b' => (2, true),
            _ => (16, false)  // widest range for unknown prefixes
        };

        bool IsDigit(char? c) => IsDigitGivenRadix(radix, c);
        void AppendDigitConsideringOverflow(ref ulong buffer, char digit, ref bool overflowOccurred, Position? errorPosition = null) =>
            AppendDigitConsideringOverflowGivenRadix(radix, ref buffer, digit, ref overflowOccurred, errorPosition);

        if (!isPrefixValid)
        {
            while (IsDigit(_scanner.CurrentCharacter))
                _scanner.Advance();
            EmitError(new InvalidNonDecimalPrefix(prefixPosition, prefix));
            return new Token(TokenType.LiteralInteger, 0ul);
        }

        if (!IsDigit(_scanner.CurrentCharacter))
        {
            EmitError(new MissingNonDecimalDigits(_scanner.CurrentPosition, prefix));
            return new Token(TokenType.LiteralInteger, 0ul);
        }

        var overflowOccurred = false;

        var integralPart = (ulong)CharToDigit(_scanner.CurrentCharacter!.Value);
        _scanner.Advance();
        while (IsDigit(_scanner.CurrentCharacter))
        {
            AppendDigitConsideringOverflow(ref integralPart, _scanner.CurrentCharacter!.Value, ref overflowOccurred);
            _scanner.Advance();
        }
        return new Token(TokenType.LiteralInteger, integralPart);
    }
}
