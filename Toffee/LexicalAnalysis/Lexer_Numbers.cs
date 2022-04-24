namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer
{
    private Token? MatchNumber()
    {
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
        void AppendDigitConsideringOverflow(ref long buffer, char digit, ref bool overflowOccurred, uint? offset = null) =>
            AppendDigitConsideringOverflowGivenRadix(10, ref buffer, digit, ref overflowOccurred, offset);

        var overflowOccurred = false;

        var integralPart = (long)CharToDigit(initialDigit);
        while (IsDigit(_scanner.CurrentCharacter))
        {
            AppendDigitConsideringOverflow(ref integralPart, _scanner.CurrentCharacter!.Value, ref overflowOccurred);
            _scanner.Advance();
        }
        if (_scanner.CurrentCharacter is not '.')  // no fractional part
            return new Token(TokenType.LiteralInteger, integralPart);

        _scanner.Advance();
        var fractionalPart = 0L;
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
        var exponentialPart = 0L;
        var exponentSign = _scanner.CurrentCharacter is '-' ? -1 : 1;
        if (_scanner.CurrentCharacter is '-' or '+')
            _scanner.Advance();
        while (IsDigit(_scanner.CurrentCharacter))
        {
            AppendDigitConsideringOverflow(ref exponentialPart, _scanner.CurrentCharacter!.Value, ref overflowOccurred);
            _scanner.Advance();
        }
        var exponentiatedNumber = integralPart * Math.Pow(10, exponentSign * exponentialPart)
            + fractionalPart * Math.Pow(10, exponentSign * exponentialPart - fractionalPartLength);
        return new Token(TokenType.LiteralFloat, exponentiatedNumber);
    }

    private Token ContinueMatchingNonDecimalInteger(char prefix)
    {
        var radix = prefix switch
        {
            'x' => 16,
            'c' => 8,
            'b' => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(prefix), prefix, "Unknown non-decimal prefix")
        };

        bool IsDigit(char? c) => IsDigitGivenRadix(radix, c);
        void AppendDigitConsideringOverflow(ref long buffer, char digit, ref bool overflowOccurred, uint? offset = null) =>
            AppendDigitConsideringOverflowGivenRadix(radix, ref buffer, digit, ref overflowOccurred, offset);

        if (!IsDigit(_scanner.CurrentCharacter))
        {
            EmitError(new NonDecimalDigitsMissing(CurrentOffset));
            return new Token(TokenType.LiteralInteger, 0L);
        }

        var overflowOccurred = false;

        var integralPart = (long)CharToDigit(_scanner.CurrentCharacter!.Value);
        _scanner.Advance();
        while (IsDigit(_scanner.CurrentCharacter))
        {
            AppendDigitConsideringOverflow(ref integralPart, _scanner.CurrentCharacter!.Value, ref overflowOccurred);
            _scanner.Advance();
        }
        return new Token(TokenType.LiteralInteger, integralPart);
    }
}
