using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceAxisOverlayState
{
    public static SurfaceAxisOverlayState Empty { get; } = new(
        [],
        null,
        [],
        0d,
        0d);

    public SurfaceAxisOverlayState(
        IReadOnlyList<SurfaceAxisState> axes,
        string? gridPlaneKey,
        IReadOnlyList<SurfaceAxisLineGeometry> gridLines,
        double anchorCornerX,
        double anchorCornerZ)
    {
        ArgumentNullException.ThrowIfNull(axes);
        ArgumentNullException.ThrowIfNull(gridLines);
        Axes = axes;
        GridPlaneKey = gridPlaneKey;
        GridLines = gridLines;
        AnchorCornerX = anchorCornerX;
        AnchorCornerZ = anchorCornerZ;
    }

    public IReadOnlyList<SurfaceAxisState> Axes { get; }

    public string? GridPlaneKey { get; }

    public IReadOnlyList<SurfaceAxisLineGeometry> GridLines { get; }

    public double AnchorCornerX { get; }

    public double AnchorCornerZ { get; }
}

internal sealed class SurfaceAxisState
{
    public SurfaceAxisState(
        string axisKey,
        SurfaceAxisLineGeometry axisLine,
        string titleText,
        Point titlePosition,
        IReadOnlyList<SurfaceAxisTickState> ticks,
        IReadOnlyList<SurfaceAxisLineGeometry> minorTicks,
        int majorTickCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(axisKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(titleText);
        ArgumentNullException.ThrowIfNull(ticks);
        ArgumentNullException.ThrowIfNull(minorTicks);

        AxisKey = axisKey;
        AxisLine = axisLine;
        TitleText = titleText;
        TitlePosition = titlePosition;
        Ticks = ticks;
        MinorTicks = minorTicks;
        MajorTickCount = majorTickCount;
    }

    public string AxisKey { get; }

    public SurfaceAxisLineGeometry AxisLine { get; }

    public string TitleText { get; }

    public Point TitlePosition { get; }

    public IReadOnlyList<SurfaceAxisTickState> Ticks { get; }

    public IReadOnlyList<SurfaceAxisLineGeometry> MinorTicks { get; }

    public int MajorTickCount { get; }
}

internal sealed class SurfaceAxisTickState
{
    public SurfaceAxisTickState(
        double value,
        string labelText,
        SurfaceAxisLineGeometry tickLine,
        Point labelPosition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(labelText);

        Value = value;
        LabelText = labelText;
        TickLine = tickLine;
        LabelPosition = labelPosition;
    }

    public double Value { get; }

    public string LabelText { get; }

    public SurfaceAxisLineGeometry TickLine { get; }

    public Point LabelPosition { get; }
}

internal readonly record struct SurfaceAxisLineGeometry(Point Start, Point End);
