namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the authoritative persisted view state for a surface chart.
/// </summary>
public readonly record struct SurfaceViewState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceViewState"/> struct.
    /// </summary>
    /// <param name="dataWindow">The authoritative data window.</param>
    /// <param name="camera">The persisted camera pose.</param>
    public SurfaceViewState(SurfaceDataWindow dataWindow, SurfaceCameraPose camera)
    {
        DataWindow = dataWindow;
        Camera = camera;
    }

    /// <summary>
    /// Gets the authoritative data window.
    /// </summary>
    public SurfaceDataWindow DataWindow { get; }

    /// <summary>
    /// Gets the persisted camera pose.
    /// </summary>
    public SurfaceCameraPose Camera { get; }

    /// <summary>
    /// Converts the authoritative data window into the viewport compatibility shell.
    /// </summary>
    /// <returns>The equivalent viewport.</returns>
    public SurfaceViewport ToViewport()
    {
        return DataWindow.ToViewport();
    }

    /// <summary>
    /// Creates the default persisted view state for the supplied dataset slice.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="dataWindow">The active data window.</param>
    /// <returns>The default view state.</returns>
    public static SurfaceViewState CreateDefault(SurfaceMetadata metadata, SurfaceDataWindow dataWindow)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedWindow = dataWindow.ClampTo(metadata);
        return new SurfaceViewState(clampedWindow, SurfaceCameraPose.CreateDefault(metadata, clampedWindow));
    }
}
