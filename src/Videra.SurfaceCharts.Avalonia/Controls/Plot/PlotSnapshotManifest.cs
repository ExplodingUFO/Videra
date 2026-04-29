namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Carries deterministic metadata for a completed Plot snapshot export.
/// </summary>
public sealed class PlotSnapshotManifest
{
    internal PlotSnapshotManifest(
        int width,
        int height,
        string outputEvidenceKind,
        string datasetEvidenceKind,
        string activeSeriesIdentity,
        PlotSnapshotFormat format,
        PlotSnapshotBackground background,
        DateTime createdUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputEvidenceKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(datasetEvidenceKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(activeSeriesIdentity);

        Width = width;
        Height = height;
        OutputEvidenceKind = outputEvidenceKind;
        DatasetEvidenceKind = datasetEvidenceKind;
        ActiveSeriesIdentity = activeSeriesIdentity;
        Format = format;
        Background = background;
        CreatedUtc = createdUtc;
    }

    /// <summary>
    /// Gets the snapshot width in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the snapshot height in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the output evidence kind that produced this snapshot.
    /// </summary>
    public string OutputEvidenceKind { get; }

    /// <summary>
    /// Gets the dataset evidence kind associated with this snapshot.
    /// </summary>
    public string DatasetEvidenceKind { get; }

    /// <summary>
    /// Gets the deterministic active-series identity at the time of capture.
    /// </summary>
    public string ActiveSeriesIdentity { get; }

    /// <summary>
    /// Gets the image format of the snapshot artifact.
    /// </summary>
    public PlotSnapshotFormat Format { get; }

    /// <summary>
    /// Gets the background behavior used for the snapshot.
    /// </summary>
    public PlotSnapshotBackground Background { get; }

    /// <summary>
    /// Gets the UTC timestamp when the snapshot was created.
    /// </summary>
    public DateTime CreatedUtc { get; }
}
