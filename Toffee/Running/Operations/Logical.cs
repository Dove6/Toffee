﻿namespace Toffee.Running.Operations;

public static class Logical
{
    public static bool? Negate(object? value)
    {
        if (value is not bool)
            return null;
        return !Casting.ToBool(value);
    }

    public static bool? Disjoin(object? first, object? second)
    {
        if (first is not bool || second is not bool)
            return null;
        return Casting.ToBool(first)!.Value || Casting.ToBool(second)!.Value;
    }

    public static bool? Conjoin(object? first, object? second)
    {
        if (first is not bool || second is not bool)
            return null;
        return Casting.ToBool(first)!.Value && Casting.ToBool(second)!.Value;
    }
}
