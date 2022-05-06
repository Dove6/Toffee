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
        if ((initialD, _scanner.CurrentCharacter) is not (('0', 'x') or ('0', 'c') or ('0', 'b')))
            return ContinueMatchingDecimalNumber(initialD);
        var prefix = _scanner.CurrentCharacter.Value;
        _scanner.Advance();
        return ContinueMatchingNonDecimalInteger(prefix);
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

    private Token ContinueMatchingNonDecimalInteger(char prefix)
    {
        // TODO: report other prefixes
        var radix = prefix switch
        {
            'x' => 16,
            'c' => 8,
            'b' => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(prefix), prefix, "Unknown non-decimal prefix")
        };

        bool IsDigit(char? c) => IsDigitGivenRadix(radix, c);
        void AppendDigitConsideringOverflow(ref ulong buffer, char digit, ref bool overflowOccurred, Position? errorPosition = null) =>
            AppendDigitConsideringOverflowGivenRadix(radix, ref buffer, digit, ref overflowOccurred, errorPosition);

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
