using Toffee.Scanning;

namespace Toffee.Logging;

public abstract class Logger
{
    protected string? SourceName;

    protected Logger(string? sourceName = null)
    {
        SourceName = sourceName;
    }

    public void LogError(Position position, string message, params object?[] attachments) =>
        Log(LogLevel.Error, position, message, attachments);
    public void LogWarning(Position position, string message, params object?[] attachments) =>
        Log(LogLevel.Warning, position, message, attachments);

    public abstract void Log(LogLevel level, Position position, string message, params object?[] attachments);
}

public enum LogLevel
{
    Error,
    Warning
}
