namespace Toffee.Running.Operations;

public static class Relational
{

    public static bool? IsLessThan(object? first, object? second)
    {
        // TODO: handle more types
        if (first is not (long or double) || second is not (long or double))
            return null;
        if (first is double || second is double)
            return Casting.ToFloat(first) < Casting.ToFloat(second);
        return Casting.ToInt(first) < Casting.ToInt(second);
    }

    public static bool? IsLessOrEqual(object? first, object? second)
    {
        // TODO: handle more types
        if (first is not (long or double) || second is not (long or double))
            return null;
        if (first is double || second is double)
            return Casting.ToFloat(first) <= Casting.ToFloat(second);
        return Casting.ToInt(first) <= Casting.ToInt(second);
    }

    public static bool? IsGreaterThan(object? first, object? second)
    {
        // TODO: handle more types
        if (first is not (long or double) || second is not (long or double))
            return null;
        if (first is double || second is double)
            return Casting.ToFloat(first) > Casting.ToFloat(second);
        return Casting.ToInt(first) > Casting.ToInt(second);
    }

    public static bool? IsGreaterOrEqual(object? first, object? second)
    {
        // TODO: handle more types
        if (first is not (long or double) || second is not (long or double))
            return null;
        if (first is double || second is double)
            return Casting.ToFloat(first) >= Casting.ToFloat(second);
        return Casting.ToInt(first) >= Casting.ToInt(second);
    }

    public static bool? IsEqualTo(object? first, object? second)
    {
        // TODO: handle more types
        if (first is null && second is null)
            return true;
        if (first is null || second is null)
            return false;
        if (first.GetType() != second.GetType())
            return false;
        return first switch
        {
            long integerFirst => integerFirst == Casting.ToInt(second),
            double doubleFirst => doubleFirst == Casting.ToFloat(second),
            string stringFirst => string.Compare(stringFirst, Casting.ToString(second), StringComparison.InvariantCulture) == 0,
            bool boolFirst => boolFirst == Casting.ToBool(second),
            _ => null
        };
    }

    public static bool? IsNotEqualTo(object? first, object? second)
    {
        var equalityCheckResult = IsEqualTo(first, second);
        return !equalityCheckResult;
    }
}
