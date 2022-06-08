namespace Toffee.Running.Operations;

public static class Character
{
    public static object? Concatenate(object? first, object? second)
    {
        if (first is not string || second is not string)
            return null;
        return Casting.ToString(first) + Casting.ToString(second);
    }
}
