using Videra.Core.Scene;

namespace Videra.Avalonia.Controls;

/// <summary>
/// Describes the outcome of a single model import initiated through <see cref="VideraView.LoadModelAsync(string, CancellationToken)" />.
/// </summary>
public sealed class ModelLoadResult
{
    private ModelLoadResult(string path, SceneDocumentEntry? entry, ModelLoadFailure? failure, TimeSpan duration)
    {
        Path = path;
        Entry = entry;
        Failure = failure;
        Duration = duration;
    }

    /// <summary>
    /// Gets the file path that was requested for import.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the loaded scene entry when import succeeded; otherwise <c>null</c>.
    /// </summary>
    public SceneDocumentEntry? Entry { get; }

    /// <summary>
    /// Gets the failure details when import did not succeed; otherwise <c>null</c>.
    /// </summary>
    public ModelLoadFailure? Failure { get; }

    /// <summary>
    /// Gets the total elapsed time spent performing the import.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets a value indicating whether the requested file imported successfully.
    /// </summary>
    public bool Succeeded => Failure is null && Entry is not null;

    /// <summary>
    /// Creates a successful single-file load result.
    /// </summary>
    public static ModelLoadResult Success(string path, SceneDocumentEntry entry, TimeSpan duration)
    {
        return new ModelLoadResult(path, entry, failure: null, duration);
    }

    /// <summary>
    /// Creates a failed single-file load result.
    /// </summary>
    public static ModelLoadResult Failed(string path, Exception exception, TimeSpan duration)
    {
        return new ModelLoadResult(path, entry: null, new ModelLoadFailure(path, exception), duration);
    }
}

/// <summary>
/// Describes the outcome of a bounded batch import initiated through <see cref="VideraView.LoadModelsAsync(IEnumerable{string}, CancellationToken)" />.
/// </summary>
public sealed class ModelLoadBatchResult
{
    public ModelLoadBatchResult(
        IReadOnlyList<SceneDocumentEntry> entries,
        IReadOnlyList<ModelLoadFailure> failures,
        TimeSpan duration)
    {
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        Failures = failures ?? throw new ArgumentNullException(nameof(failures));
        Duration = duration;
    }

    /// <summary>
    /// Gets the scene entries applied to the active scene when the batch succeeds.
    /// </summary>
    public IReadOnlyList<SceneDocumentEntry> Entries { get; }

    /// <summary>
    /// Gets the file-level failures captured during the batch.
    /// </summary>
    public IReadOnlyList<ModelLoadFailure> Failures { get; }

    /// <summary>
    /// Gets the total elapsed time for the batch import.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets a value indicating whether every requested file imported successfully.
    /// </summary>
    public bool Succeeded => Failures.Count == 0;
}

/// <summary>
/// Describes a single-file model import failure.
/// </summary>
public sealed class ModelLoadFailure
{
    public ModelLoadFailure(string path, Exception exception)
    {
        Path = path;
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        ErrorMessage = exception.Message;
    }

    /// <summary>
    /// Gets the file path associated with the failure.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the human-readable failure message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the underlying exception thrown during import.
    /// </summary>
    public Exception Exception { get; }
}
