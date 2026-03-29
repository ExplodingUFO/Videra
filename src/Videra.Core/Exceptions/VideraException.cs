namespace Videra.Core.Exceptions;

/// <summary>
/// Base exception for all Videra domain exceptions.
/// Provides structured diagnostic fields for programmatic consumption.
/// </summary>
public class VideraException : Exception
{
    /// <summary>
    /// The operation that failed (e.g. "LoadModel", "Initialize", "CreateBuffer").
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Optional structured key-value pairs for diagnostic context.
    /// </summary>
    public IReadOnlyDictionary<string, string> Context { get; }

    public VideraException(string message, string operation, IReadOnlyDictionary<string, string?>? context = null)
        : base(message)
    {
        Operation = operation;
        Context = context != null
            ? new Dictionary<string, string>(
                context
                    .Where(kv => kv.Value != null)
                    .ToDictionary(kv => kv.Key, kv => kv.Value!))
            : new Dictionary<string, string>();
    }

    public VideraException(string message, string operation, Exception inner, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, inner)
    {
        Operation = operation;
        Context = context != null
            ? new Dictionary<string, string>(
                context
                    .Where(kv => kv.Value != null)
                    .ToDictionary(kv => kv.Key, kv => kv.Value!))
            : new Dictionary<string, string>();
    }
}
