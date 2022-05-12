using Toffee.Scanning;

namespace Toffee.ErrorHandling;

public abstract record Error(Position Position);
public abstract record Warning(Position Position);
