namespace Toffee.Running.Operations;

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
            return (double)augend + (double)addend;
        return (long)augend + (long)addend;
    }

    public static object? Subtract(object? minuend, object? subtrahend)
    {
        if (minuend is not (long or double) || subtrahend is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (minuend is double || subtrahend is double)
            return (double)minuend - (double)subtrahend;
        return (long)minuend - (long)subtrahend;
    }

    public static object? Multiply(object? multiplier, object? multiplicand)
    {
        if (multiplier is not (long or double) || multiplicand is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (multiplier is double || multiplicand is double)
            return (double)multiplier * (double)multiplicand;
        return (long)multiplier * (long)multiplicand;
    }

    public static object? Divide(object? dividend, object? divisor)
    {
        if (dividend is not (long or double) || divisor is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (dividend is double || divisor is double)
            return (double)dividend / (double)divisor;
        if (divisor is 0L)
            throw new NotImplementedException();
        return (long)dividend / (long)divisor;
    }

    public static object? Remainder(object? dividend, object? divisor)
    {
        if (dividend is not (long or double) || divisor is not (long or double))
            return null;
        // TODO: describe promotion rules
        if (dividend is double || divisor is double)
            return (double)dividend % (double)divisor;
        if (divisor is 0L)
            throw new NotImplementedException();
        return (long)dividend % (long)divisor;
    }

    public static object? Exponentiate(object? value, object? exponent)
    {
        if (value is not (long or double) || exponent is not (long or double))
            return null;
        // TODO: describe promotion rules
        var result = Math.Pow((double)value, (double)exponent);
        if (value is double || exponent is double)
            return result;
        // TODO: better handling for integers
        return (long)result;
    }
}
