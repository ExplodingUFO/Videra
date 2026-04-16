namespace Videra.SurfaceCharts.Rendering;

/// <summary>
/// Represents the lightweight projection angles shared between persisted chart view-state and rendering.
/// </summary>
public readonly record struct SurfaceChartProjectionSettings(double YawDegrees, double PitchDegrees)
{
    /// <summary>
    /// Gets the default chart projection settings.
    /// </summary>
    public static SurfaceChartProjectionSettings Default { get; } = new(0d, 0d);
}
