namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Captures the parameters for a Plot-owned snapshot export request.
/// </summary>
public sealed class PlotSnapshotRequest
{
    /// <summary>
    /// Initializes a new <see cref="PlotSnapshotRequest"/> with the specified dimensions, scale, background, and format.
    /// </summary>
    /// <param name="width">The target snapshot width in pixels. Must be positive.</param>
    /// <param name="height">The target snapshot height in pixels. Must be positive.</param>
    /// <param name="scale">The DPI scale factor. Must be positive.</param>
    /// <param name="background">The background behavior for the snapshot.</param>
    /// <param name="format">The target image format.</param>
    public PlotSnapshotRequest(
        int width,
        int height,
        double scale,
        PlotSnapshotBackground background,
        PlotSnapshotFormat format)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(scale);

        Width = width;
        Height = height;
        Scale = scale;
        Background = background;
        Format = format;
    }

    /// <summary>
    /// Gets the target snapshot width in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the target snapshot height in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the DPI scale factor.
    /// </summary>
    public double Scale { get; }

    /// <summary>
    /// Gets the background behavior for the snapshot.
    /// </summary>
    public PlotSnapshotBackground Background { get; }

    /// <summary>
    /// Gets the target image format.
    /// </summary>
    public PlotSnapshotFormat Format { get; }
}
