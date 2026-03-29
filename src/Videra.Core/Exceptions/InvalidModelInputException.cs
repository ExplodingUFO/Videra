namespace Videra.Core.Exceptions;

/// <summary>
/// Thrown when model file input is invalid (null/empty path, missing file,
/// unsupported format, directory passed as file, etc.).
/// </summary>
public class InvalidModelInputException : VideraException
{
    public InvalidModelInputException(string message, string operation, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, context)
    {
    }

    public InvalidModelInputException(string message, string operation, Exception inner, IReadOnlyDictionary<string, string?>? context = null)
        : base(message, operation, inner, context)
    {
    }
}
