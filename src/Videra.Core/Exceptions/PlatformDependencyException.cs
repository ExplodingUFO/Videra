namespace Videra.Core.Exceptions;

/// <summary>
/// Thrown when a platform dependency is missing or unavailable
/// (e.g. native library, driver, runtime).
/// </summary>
public class PlatformDependencyException : VideraException
{
    /// <summary>
    /// The platform where the failure occurred (e.g. "Windows", "Linux", "macOS").
    /// </summary>
    public string Platform { get; }

    /// <summary>
    /// Platform error code if available (e.g. HRESULT for Windows, VkResult for Linux).
    /// </summary>
    public int? ErrorCode { get; }

    public PlatformDependencyException(string message, string operation, string platform, int? errorCode = null, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, context)
    {
        Platform = platform;
        ErrorCode = errorCode;
    }

    public PlatformDependencyException(string message, string operation, string platform, Exception inner, int? errorCode = null, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, inner, context)
    {
        Platform = platform;
        ErrorCode = errorCode;
    }
}
