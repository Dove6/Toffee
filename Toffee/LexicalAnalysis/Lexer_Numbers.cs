namespace Toffee.LexicalAnalysis;

public partial class Lexer
{
    private Token? MatchNumber()
    {
        var radix = 10;
        if (!IsDigitGivenRadix(radix, _scanner.CurrentCharacter))
            return null;

        var initialD = _scanner.CurrentCharacter!.Value;
        _scanner.Advance();
        if ((initialD, _scanner.CurrentCharacter) is ('0', 'x') or ('0', 'c') or ('0', 'b'))
        {
#pragma warning disable CS8509
            radix = _scanner.CurrentCharacter switch
            {
                'x' => 16,
                'c' => 8,
                'b' => 2
            };
#pragma warning restore CS8509
            _scanner.Advance();
        }
        return radix == 10
            ? ContinueMatchingDecimalNumber(initialD)
            : ContinueMatchingNonDecimalInteger(radix);
    }

    private Token ContinueMatchingDecimalNumber(char initialDigit)
    {
        static bool IsDigit(char? c) => IsDigitGivenRadix(10, c);

        static (long, OverflowException?) AppendDigit(long buffer, char digit) =>
            AppendDigitGivenRadix(10, buffer, digit);

        OverflowException? overflowException = null; // TODO: handle this
        var integralPart = (long)CharToDigit(initialDigit);
        while (IsDigit(_scanner.CurrentCharacter))
        {
            if (overflowException is null)
                (integralPart, overflowException) = AppendDigit(integralPart, _scanner.CurrentCharacter!.Value);
            _scanner.Advance();
        }
        if (_scanner.CurrentCharacter is not '.')
            // no fractional part
            return new Token(TokenType.LiteralInteger, integralPart);

        _scanner.Advance();
        var fractionalPart = 0L;
        var fractionalPartLength = 0;
        while (IsDigit(_scanner.CurrentCharacter))
        {
            if (overflowException is null)
                (fractionalPart, overflowException) = AppendDigit(fractionalPart, _scanner.CurrentCharacter.Value);
            if (overflowException is null)
                fractionalPartLength++;
            _scanner.Advance();
        }
        if (_scanner.CurrentCharacter is not ('e' or 'E'))
            // no exponential part
            return new Token(TokenType.LiteralFloat,
                integralPart + fractionalPart / Math.Pow(10, fractionalPartLength));

        _scanner.Advance();
        var exponentialPart = 0L;
        var exponentSign = _scanner.CurrentCharacter is '-' ? -1 : 1;
        if (_scanner.CurrentCharacter is '-' or '+')
            _scanner.Advance();
        while (IsDigit(_scanner.CurrentCharacter))
        {
            if (overflowException is null)
                (exponentialPart, overflowException) = AppendDigit(exponentialPart, _scanner.CurrentCharacter.Value);
            _scanner.Advance();
        }
        var exponentiatedNumber = integralPart * Math.Pow(10, exponentSign * exponentialPart)
                                  + fractionalPart * Math.Pow(10,
                                      exponentSign * exponentialPart - fractionalPartLength);
        return new Token(TokenType.LiteralFloat, exponentiatedNumber);
    }

    private Token ContinueMatchingNonDecimalInteger(int radix)
    {
        bool IsDigit(char? c) => IsDigitGivenRadix(radix, c);

        (long, OverflowException?) AppendDigit(long buffer, char digit) =>
            AppendDigitGivenRadix(radix, buffer, digit);

        if (!IsDigit(_scanner.CurrentCharacter))
            new string("Expected digits"); // TODO: error

        OverflowException? overflowException = null; // TODO: handle this
        var integralPart = (long)CharToDigit(_scanner.CurrentCharacter!.Value);
        _scanner.Advance();
        while (IsDigit(_scanner.CurrentCharacter))
        {
            if (overflowException is null)
                (integralPart, overflowException) = AppendDigit(integralPart, _scanner.CurrentCharacter.Value);
            _scanner.Advance();
        }
        return new Token(TokenType.LiteralInteger, integralPart);
    }
}
