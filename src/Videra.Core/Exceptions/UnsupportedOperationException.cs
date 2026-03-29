namespace Videra.Core.Exceptions;

/// <summary>
/// Thrown when an unsupported operation is attempted (replaces NotImplementedException
/// in public-facing paths). Message is UI/Demo-consumable.
/// </summary>
public class UnsupportedOperationException : VideraException
{
    public UnsupportedOperationException(string message, string operation, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, context)
    {
    }

    public UnsupportedOperationException(string message, string operation, Exception inner, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, inner, context)
    {
    }
}
