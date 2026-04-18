namespace Videra.Avalonia.Controls;

/// <summary>
/// Describes the outcome of exporting a viewer inspection snapshot to an image artifact.
/// </summary>
public sealed class VideraSnapshotExportResult
{
    private VideraSnapshotExportResult(
        string path,
        uint width,
        uint height,
        TimeSpan duration,
        Exception? failure)
    {
        Path = path;
        Width = width;
        Height = height;
        Duration = duration;
        Failure = failure;
    }

    public string Path { get; }

    public uint Width { get; }

    public uint Height { get; }

    public TimeSpan Duration { get; }

    public Exception? Failure { get; }

    public bool Succeeded => Failure is null;

    public static VideraSnapshotExportResult Success(string path, uint width, uint height, TimeSpan duration)
    {
        return new VideraSnapshotExportResult(path, width, height, duration, failure: null);
    }

    public static VideraSnapshotExportResult Failed(string path, uint width, uint height, TimeSpan duration, Exception failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new VideraSnapshotExportResult(path, width, height, duration, failure);
    }
}
