namespace Toffee.Running.Operations;

public static class Logical
{
    public static bool? Negate(bool? value)
    {
        return !value;
    }

    public static bool? Disjoin(bool? first, Lazy<bool?> second)
    {
        if (first is true or null)
            return first;
        if (second.Value is null)
            return null;
        return first.Value || second.Value.Value;
    }

    public static bool? Conjoin(bool? first, Lazy<bool?> second)
    {
        if (first is false or null)
            return first;
        if (second.Value is null)
            return null;
        return first.Value && second.Value.Value;
    }
}
