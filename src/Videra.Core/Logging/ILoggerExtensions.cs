using Microsoft.Extensions.Logging;

namespace Videra.Core.Logging;

/// <summary>
/// Logger extension methods for structured logging with consistent templates.
/// Provides helper methods that map component-tagged messages to structured log properties.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs an informational message tagged with a component name.
    /// </summary>
    public static void LogComponentInfo(this ILogger logger, string component, string message)
    {
        logger.LogInformation("[{Component}] {Message}", component, message);
    }

    /// <summary>
    /// Logs a debug message with component, action, and details.
    /// </summary>
    public static void LogComponentDebug(this ILogger logger, string component, string action, string details)
    {
        logger.LogDebug("[{Component}] {Action}: {Details}", component, action, details);
    }

    /// <summary>
    /// Logs an error message with component, action, and optional exception.
    /// </summary>
    public static void LogComponentError(this ILogger logger, string component, string action, string error, Exception? ex = null)
    {
        logger.LogError(ex, "[{Component}] {Action} failed: {Error}", component, action, error);
    }

    /// <summary>
    /// Logs a warning message tagged with a component name.
    /// </summary>
    public static void LogComponentWarning(this ILogger logger, string component, string message)
    {
        logger.LogWarning("[{Component}] {Message}", component, message);
    }
}
