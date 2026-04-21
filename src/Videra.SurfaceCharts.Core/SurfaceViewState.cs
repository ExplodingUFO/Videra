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
    /// <param name="displaySpace">The selected display-space contract.</param>
    public SurfaceViewState(
        SurfaceDataWindow dataWindow,
        SurfaceCameraPose camera,
        SurfaceDisplaySpace displaySpace = SurfaceDisplaySpace.Raw)
    {
        DataWindow = dataWindow;
        Camera = camera;
        DisplaySpace = displaySpace;
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
    /// Gets the display-space contract associated with this view state.
    /// </summary>
    public SurfaceDisplaySpace DisplaySpace { get; }

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
    /// <param name="displaySpace">The display-space contract to persist.</param>
    /// <returns>The default view state.</returns>
    public static SurfaceViewState CreateDefault(
        SurfaceMetadata metadata,
        SurfaceDataWindow dataWindow,
        SurfaceDisplaySpace displaySpace = SurfaceDisplaySpace.Raw)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedWindow = dataWindow.ClampTo(metadata);
        return new SurfaceViewState(
            clampedWindow,
            SurfaceCameraPose.CreateDefault(metadata, clampedWindow),
            displaySpace);
    }
}
