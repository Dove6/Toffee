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
        static (long, OverflowException?) AppendDigit(long buffer, char digit) =>
            AppendDigitGivenRadix(10, buffer, digit);

        OverflowException? overflowException = null;
        void AppendDigitConsideringOverflow(ref long buffer)
        {
            if (overflowException is null)
            {
                (buffer, overflowException) = AppendDigit(buffer, _scanner.CurrentCharacter!.Value);
                if (overflowException is not null)
                    EmitError(new NumberLiteralTooLarge(CurrentOffset));
            }
            _scanner.Advance();
        }

        var integralPart = (long)CharToDigit(initialDigit);
        while (IsDigit(_scanner.CurrentCharacter))
            AppendDigitConsideringOverflow(ref integralPart);
        if (_scanner.CurrentCharacter is not '.')  // no fractional part
            return new Token(TokenType.LiteralInteger, integralPart);

        _scanner.Advance();
        var fractionalPart = 0L;
        var fractionalPartLength = 0;
        while (IsDigit(_scanner.CurrentCharacter))
        {
            AppendDigitConsideringOverflow(ref fractionalPart);
            if (overflowException is null)
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
            AppendDigitConsideringOverflow(ref exponentialPart);
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
        (long, OverflowException?) AppendDigit(long buffer, char digit) =>
            AppendDigitGivenRadix(radix, buffer, digit);

        if (!IsDigit(_scanner.CurrentCharacter))
        {
            EmitError(new NonDecimalDigitsMissing(CurrentOffset));
            return new Token(TokenType.LiteralInteger, 0L);
        }

        OverflowException? overflowException = null;
        void EmitOverflowErrorConditionally()
        {
            if (overflowException is not null)
                EmitError(new NumberLiteralTooLarge(CurrentOffset));
        }

        var integralPart = (long)CharToDigit(_scanner.CurrentCharacter!.Value);
        _scanner.Advance();
        while (IsDigit(_scanner.CurrentCharacter))
        {
            if (overflowException is null)
            {
                (integralPart, overflowException) = AppendDigit(integralPart, _scanner.CurrentCharacter.Value);
                EmitOverflowErrorConditionally();
            }
            _scanner.Advance();
        }
        return new Token(TokenType.LiteralInteger, integralPart);
    }
}
