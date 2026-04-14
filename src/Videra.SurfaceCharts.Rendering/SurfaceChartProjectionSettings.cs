namespace Videra.SurfaceCharts.Rendering;

public readonly record struct SurfaceChartProjectionSettings(double YawDegrees, double PitchDegrees)
{
    public static SurfaceChartProjectionSettings Default { get; } = new(0d, 0d);
}
