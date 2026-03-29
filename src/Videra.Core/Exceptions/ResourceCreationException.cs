namespace Videra.Core.Exceptions;

/// <summary>
/// Thrown when a GPU resource creation fails (vertex buffer, index buffer, etc.).
/// </summary>
public class ResourceCreationException : VideraException
{
    public ResourceCreationException(string message, string operation, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, context)
    {
    }

    public ResourceCreationException(string message, string operation, Exception inner, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, inner, context)
    {
    }
}
