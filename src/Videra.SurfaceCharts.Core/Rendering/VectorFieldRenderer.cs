using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Stateless renderer that builds a vector field render scene from chart data.
/// </summary>
public static class VectorFieldRenderer
{
    public static VectorFieldRenderScene BuildScene(VectorFieldChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var arrows = new List<VectorFieldRenderArrow>(data.PointCount);

        foreach (var point in data.Points)
        {
            var t = data.MagnitudeRange.Span > 0d
                ? (point.Magnitude - data.MagnitudeRange.Minimum) / data.MagnitudeRange.Span
                : 0d;
            var color = InterpolateColor(t);
            arrows.Add(new VectorFieldRenderArrow(point.Position, point.Direction, point.Magnitude, color));
        }

        return new VectorFieldRenderScene(data.PointCount, arrows);
    }

    /// <summary>
    /// Maps a normalized value (0..1) to a blue-to-red ARGB color.
    /// </summary>
    private static uint InterpolateColor(double t)
    {
        t = Math.Clamp(t, 0d, 1d);
        var r = (byte)(t * 255);
        var b = (byte)((1d - t) * 255);
        return (uint)(0xFF000000 | ((uint)r << 16) | ((uint)0 << 8) | (uint)b);
    }
}
