namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the persisted view state for a surface chart.
/// </summary>
public sealed record SurfaceViewState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceViewState"/> class.
    /// </summary>
    public SurfaceViewState(SurfaceDataWindow dataWindow, SurfaceCameraPose camera)
    {
        DataWindow = dataWindow ?? throw new ArgumentNullException(nameof(dataWindow));
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    /// <summary>
    /// Gets the current data window used for LOD and tile selection.
    /// </summary>
    public SurfaceDataWindow DataWindow { get; }

    /// <summary>
    /// Gets the current 3D camera pose.
    /// </summary>
    public SurfaceCameraPose Camera { get; }
}
