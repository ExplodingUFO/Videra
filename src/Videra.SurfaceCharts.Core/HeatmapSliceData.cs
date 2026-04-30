using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Selects the axis along which a heatmap slice is taken.
/// </summary>
public enum HeatmapSliceAxis
{
    /// <summary>Slice perpendicular to the X axis.</summary>
    X,
    /// <summary>Slice perpendicular to the Y (value) axis.</summary>
    Y,
    /// <summary>Slice perpendicular to the Z (depth) axis.</summary>
    Z,
}

/// <summary>
/// Represents one immutable heatmap-slice dataset.
/// </summary>
public sealed class HeatmapSliceData
{
    public HeatmapSliceData(
        SurfaceScalarField field,
        HeatmapSliceAxis axis,
        double position,
        SurfaceColorMap? colorMap = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (position < 0d || position > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be normalized between 0 and 1.");
        }
        Field = field;
        Axis = axis;
        Position = position;
        ColorMap = colorMap;
    }

    public SurfaceScalarField Field { get; }
    public HeatmapSliceAxis Axis { get; }
    public double Position { get; }
    public SurfaceColorMap? ColorMap { get; }
}
