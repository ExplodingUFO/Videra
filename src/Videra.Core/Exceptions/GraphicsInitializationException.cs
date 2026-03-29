namespace Videra.Core.Exceptions;

/// <summary>
/// Thrown when a graphics initialization step fails (backend init, device creation, etc.).
/// </summary>
public class GraphicsInitializationException : VideraException
{
    /// <summary>
    /// Platform error code if available (e.g. HRESULT for Windows, VkResult for Linux).
    /// </summary>
    public int? ErrorCode { get; }

    public GraphicsInitializationException(string message, string operation, int? errorCode = null, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, context)
    {
        ErrorCode = errorCode;
    }

    public GraphicsInitializationException(string message, string operation, Exception inner, int? errorCode = null, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, inner, context)
    {
        ErrorCode = errorCode;
    }
}
