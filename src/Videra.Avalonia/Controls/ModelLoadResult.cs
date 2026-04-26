using Videra.Core.Scene;

namespace Videra.Avalonia.Controls;

/// <summary>
/// Describes the outcome of a single model import initiated through <see cref="VideraView.LoadModelAsync(string, CancellationToken)" />.
/// </summary>
public sealed class ModelLoadResult
{
    private ModelLoadResult(
        string path,
        SceneDocumentEntry? entry,
        ModelLoadFailure? failure,
        TimeSpan duration,
        IReadOnlyList<ModelImportDiagnostic>? diagnostics,
        TimeSpan importDuration)
    {
        Path = path;
        Entry = entry;
        Failure = failure;
        Duration = duration;
        Diagnostics = diagnostics ?? Array.Empty<ModelImportDiagnostic>();
        ImportDuration = importDuration;
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
    /// Gets importer diagnostics captured while loading the file.
    /// </summary>
    public IReadOnlyList<ModelImportDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets the time reported by the importer itself, excluding scene application work when available.
    /// </summary>
    public TimeSpan ImportDuration { get; }

    /// <summary>
    /// Gets a value indicating whether the requested file imported successfully.
    /// </summary>
    public bool Succeeded => Failure is null && Entry is not null;

    /// <summary>
    /// Creates a successful single-file load result.
    /// </summary>
    public static ModelLoadResult Success(
        string path,
        SceneDocumentEntry entry,
        TimeSpan duration,
        IReadOnlyList<ModelImportDiagnostic>? diagnostics = null,
        TimeSpan importDuration = default)
    {
        return new ModelLoadResult(path, entry, failure: null, duration, diagnostics, importDuration);
    }

    /// <summary>
    /// Creates a failed single-file load result.
    /// </summary>
    public static ModelLoadResult Failed(
        string path,
        Exception exception,
        TimeSpan duration,
        IReadOnlyList<ModelImportDiagnostic>? diagnostics = null,
        TimeSpan importDuration = default)
    {
        return new ModelLoadResult(path, entry: null, new ModelLoadFailure(path, exception), duration, diagnostics, importDuration);
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
        TimeSpan duration,
        IReadOnlyList<ModelLoadFileResult>? results = null)
    {
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        Failures = failures ?? throw new ArgumentNullException(nameof(failures));
        Duration = duration;
        Results = results ?? CreateResults(entries, failures);
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
    /// Gets the per-file import outcomes, including successful imports that were not applied because another file failed.
    /// </summary>
    public IReadOnlyList<ModelLoadFileResult> Results { get; }

    /// <summary>
    /// Gets a value indicating whether every requested file imported successfully.
    /// </summary>
    public bool Succeeded => Failures.Count == 0;

    private static IReadOnlyList<ModelLoadFileResult> CreateResults(
        IReadOnlyList<SceneDocumentEntry> entries,
        IReadOnlyList<ModelLoadFailure> failures)
    {
        return entries
            .Select(static entry => ModelLoadFileResult.Success(
                entry.ImportedAsset?.FilePath ?? entry.Name,
                entry,
                Array.Empty<ModelImportDiagnostic>(),
                importDuration: TimeSpan.Zero,
                entry.ImportedAsset?.Metrics))
            .Concat(failures.Select(static failure => ModelLoadFileResult.Failed(
                failure.Path,
                failure,
                Array.Empty<ModelImportDiagnostic>(),
                importDuration: TimeSpan.Zero)))
            .ToArray();
    }
}

/// <summary>
/// Describes the import and scene-application outcome for one file in a batch load.
/// </summary>
public sealed class ModelLoadFileResult
{
    private ModelLoadFileResult(
        string path,
        SceneDocumentEntry? entry,
        ModelLoadFailure? failure,
        IReadOnlyList<ModelImportDiagnostic>? diagnostics,
        TimeSpan importDuration,
        SceneAssetMetrics? assetMetrics)
    {
        Path = path;
        Entry = entry;
        Failure = failure;
        Diagnostics = diagnostics ?? Array.Empty<ModelImportDiagnostic>();
        ImportDuration = importDuration;
        AssetMetrics = assetMetrics;
    }

    public string Path { get; }

    public SceneDocumentEntry? Entry { get; }

    public ModelLoadFailure? Failure { get; }

    public IReadOnlyList<ModelImportDiagnostic> Diagnostics { get; }

    public TimeSpan ImportDuration { get; }

    public SceneAssetMetrics? AssetMetrics { get; }

    public bool Imported => Failure is null;

    public bool Applied => Entry is not null;

    public static ModelLoadFileResult Success(
        string path,
        SceneDocumentEntry? entry,
        IReadOnlyList<ModelImportDiagnostic>? diagnostics,
        TimeSpan importDuration,
        SceneAssetMetrics? assetMetrics = null)
    {
        return new ModelLoadFileResult(path, entry, failure: null, diagnostics, importDuration, assetMetrics);
    }

    public static ModelLoadFileResult Failed(
        string path,
        ModelLoadFailure failure,
        IReadOnlyList<ModelImportDiagnostic>? diagnostics,
        TimeSpan importDuration)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new ModelLoadFileResult(path, entry: null, failure, diagnostics, importDuration, assetMetrics: null);
    }
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
