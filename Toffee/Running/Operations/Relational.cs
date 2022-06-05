using System.Globalization;

namespace Toffee.Running.Operations;

public static class Relational
{

    public static bool? IsLessThan(object? first, object? second)
    {
        // TODO: handle more types
        if (first is not (long or double) || second is not (long or double))
            return null;
        if (first is double || second is double)
            return (double)first < (double)second;
        return (long)first < (long)second;
    }

    public static bool? IsLessOrEqual(object? first, object? second)
    {
        // TODO: handle more types
        if (first is not (long or double) || second is not (long or double))
            return null;
        if (first is double || second is double)
            return (double)first <= (double)second;
        return (long)first <= (long)second;
    }

    public static bool? IsGreaterThan(object? first, object? second)
    {
        // TODO: handle more types
        if (first is not (long or double) || second is not (long or double))
            return null;
        if (first is double || second is double)
            return (double)first > (double)second;
        return (long)first > (long)second;
    }

    public static bool? IsGreaterOrEqual(object? first, object? second)
    {
        // TODO: handle more types
        if (first is not (long or double) || second is not (long or double))
            return null;
        if (first is double || second is double)
            return (double)first >= (double)second;
        return (long)first >= (long)second;
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
            long integerFirst => integerFirst == (long)second,
            double doubleFirst => doubleFirst == (double)second,
            string stringFirst => string.Compare(stringFirst, (string)second, StringComparison.InvariantCulture) == 0,
            bool boolFirst => boolFirst == (bool)second,
            _ => null
        };
    }

    public static bool? IsNotEqualTo(object? first, object? second)
    {
        var equalityCheckResult = IsEqualTo(first, second);
        return !equalityCheckResult;
    }
}
