﻿namespace Toffee.Running.Operations;

public static class Arithmetical
{
    public static object? Negate(object? value)
    {
        return value switch
        {
            long integerValue => -integerValue,
            double floatValue => -floatValue,
            _ => null
        };
    }

    public static object? Add(object? augend, object? addend)
    {
        if (augend is not (long or double) || addend is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (augend is double || addend is double)
            return Casting.ToFloat(augend) + Casting.ToFloat(addend);
        return unchecked(Casting.ToInt(augend) + Casting.ToInt(addend));
    }

    public static object? Subtract(object? minuend, object? subtrahend)
    {
        if (minuend is not (long or double) || subtrahend is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (minuend is double || subtrahend is double)
            return Casting.ToFloat(minuend) - Casting.ToFloat(subtrahend);
        return unchecked(Casting.ToInt(minuend) - Casting.ToInt(subtrahend));
    }

    public static object? Multiply(object? multiplier, object? multiplicand)
    {
        if (multiplier is not (long or double) || multiplicand is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (multiplier is double || multiplicand is double)
            return Casting.ToFloat(multiplier) * Casting.ToFloat(multiplicand);
        return unchecked(Casting.ToInt(multiplier) * Casting.ToInt(multiplicand));
    }

    public static object? Divide(object? dividend, object? divisor)
    {
        if (dividend is not (long or double) || divisor is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (dividend is double || divisor is double)
            return Casting.ToFloat(dividend) / Casting.ToFloat(divisor);
        if (divisor is 0L)
            throw new RunnerException(new ZeroDivision());
        return Casting.ToInt(dividend) / Casting.ToInt(divisor);
    }

    public static object? Remainder(object? dividend, object? divisor)
    {
        if (dividend is not (long or double) || divisor is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (dividend is double || divisor is double)
            return Casting.ToFloat(dividend) % Casting.ToFloat(divisor);
        if (divisor is 0L)
            throw new RunnerException(new ZeroDivision());
        return Casting.ToInt(dividend) % Casting.ToInt(divisor);
    }

    public static object? Exponentiate(object? value, object? exponent)
    {
        if (value is not (long or double) || exponent is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (value is double || exponent is double || Casting.ToInt(exponent)!.Value < 0)
            return Math.Pow(Casting.ToFloat(value)!.Value, Casting.ToFloat(exponent)!.Value);
        return ExponentiateInternal(Casting.ToInt(value)!.Value, Casting.ToInt(exponent)!.Value);
    }

    private static long ExponentiateInternal(long value, long exponent)
    {
        if (exponent <= 0)
            return 1;
        var result = value;
        while (exponent-- > 1)
            result = unchecked(result * value);
        return result;
    }
}
