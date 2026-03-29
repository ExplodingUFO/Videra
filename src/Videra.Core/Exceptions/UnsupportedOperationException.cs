namespace Videra.Core.Exceptions;

/// <summary>
/// Thrown when an unsupported operation is attempted (replaces NotImplementedException
/// in public-facing paths). Carries platform and operation info for diagnostics.
/// Message is UI/Demo-consumable.
/// </summary>
public class UnsupportedOperationException : VideraException
{
    /// <summary>
    /// The platform where this operation is unsupported (e.g. "Windows", "Linux", "macOS").
    /// </summary>
    public string Platform { get; }

    public UnsupportedOperationException(string message, string operation, string platform, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, context)
    {
        Platform = platform;
    }

    public UnsupportedOperationException(string message, string operation, string platform, Exception inner, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, inner, context)
    {
        Platform = platform;
    }
}
