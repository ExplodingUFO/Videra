namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Represents the outcome of a Plot snapshot export, carrying either a successful artifact path and manifest
/// or a diagnostic describing the failure.
/// </summary>
public sealed class PlotSnapshotResult
{
    internal PlotSnapshotResult(
        bool succeeded,
        string? path,
        PlotSnapshotManifest? manifest,
        PlotSnapshotDiagnostic? failure,
        TimeSpan duration)
    {
        if (succeeded)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(manifest);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(failure);
        }

        Succeeded = succeeded;
        Path = path;
        Manifest = manifest;
        Failure = failure;
        Duration = duration;
    }

    /// <summary>
    /// Gets whether the snapshot export succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the file path of the exported snapshot artifact, or <c>null</c> when the export failed.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// Gets the deterministic manifest for the exported snapshot, or <c>null</c> when the export failed.
    /// </summary>
    public PlotSnapshotManifest? Manifest { get; }

    /// <summary>
    /// Gets the diagnostic describing why the export failed, or <c>null</c> when the export succeeded.
    /// </summary>
    public PlotSnapshotDiagnostic? Failure { get; }

    /// <summary>
    /// Gets the time taken to complete the snapshot export attempt.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Creates a successful snapshot result with the specified artifact path, manifest, and duration.
    /// </summary>
    /// <param name="path">The file path of the exported snapshot artifact.</param>
    /// <param name="manifest">The deterministic manifest for the exported snapshot.</param>
    /// <param name="duration">The time taken to complete the export.</param>
    /// <returns>A successful <see cref="PlotSnapshotResult"/>.</returns>
    internal static PlotSnapshotResult Success(string path, PlotSnapshotManifest manifest, TimeSpan duration)
    {
        return new PlotSnapshotResult(succeeded: true, path, manifest, failure: null, duration);
    }

    /// <summary>
    /// Creates a failed snapshot result with the specified diagnostic and duration.
    /// </summary>
    /// <param name="failure">The diagnostic describing why the export failed.</param>
    /// <param name="duration">The time taken to complete the export attempt.</param>
    /// <returns>A failed <see cref="PlotSnapshotResult"/>.</returns>
    internal static PlotSnapshotResult Failed(PlotSnapshotDiagnostic failure, TimeSpan duration)
    {
        return new PlotSnapshotResult(succeeded: false, path: null, manifest: null, failure, duration);
    }
}
