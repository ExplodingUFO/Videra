namespace Videra.Core.Exceptions;

/// <summary>
/// Thrown when a pipeline creation step fails.
/// </summary>
public class PipelineCreationException : VideraException
{
    public PipelineCreationException(string message, string operation, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, context)
    {
    }

    public PipelineCreationException(string message, string operation, Exception inner, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, inner, context)
    {
    }
}
