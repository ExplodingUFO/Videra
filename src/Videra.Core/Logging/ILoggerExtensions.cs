using Microsoft.Extensions.Logging;

namespace Videra.Core.Logging;

/// <summary>
/// Logger extension methods for structured logging with consistent templates.
/// Provides helper methods that map component-tagged messages to structured log properties.
/// </summary>
public static partial class LoggerExtensions
{
    /// <summary>
    /// Logs an informational message tagged with a component name.
    /// </summary>
    public static void LogComponentInfo(this ILogger logger, string component, string message)
    {
        Log.ComponentInfo(logger, component, message);
    }

    /// <summary>
    /// Logs a debug message with component, action, and details.
    /// </summary>
    public static void LogComponentDebug(this ILogger logger, string component, string action, string details)
    {
        Log.ComponentDebug(logger, component, action, details);
    }

    /// <summary>
    /// Logs an error message with component, action, and optional exception.
    /// </summary>
    public static void LogComponentError(this ILogger logger, string component, string action, string error, Exception? ex = null)
    {
        Log.ComponentError(logger, component, action, error, ex);
    }

    /// <summary>
    /// Logs a warning message tagged with a component name.
    /// </summary>
    public static void LogComponentWarning(this ILogger logger, string component, string message)
    {
        Log.ComponentWarning(logger, component, message);
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[{Component}] {Message}")]
        public static partial void ComponentInfo(ILogger logger, string component, string message);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "[{Component}] {Action}: {Details}")]
        public static partial void ComponentDebug(ILogger logger, string component, string action, string details);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "[{Component}] {Action} failed: {Error}")]
        public static partial void ComponentError(ILogger logger, string component, string action, string error, Exception? exception);

        [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "[{Component}] {Message}")]
        public static partial void ComponentWarning(ILogger logger, string component, string message);
    }
}
