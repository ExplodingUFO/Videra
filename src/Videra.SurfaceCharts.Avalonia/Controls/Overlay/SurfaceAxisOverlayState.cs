using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceAxisOverlayState
{
    public static SurfaceAxisOverlayState Empty { get; } = new([]);

    public SurfaceAxisOverlayState(IReadOnlyList<SurfaceAxisState> axes)
    {
        ArgumentNullException.ThrowIfNull(axes);
        Axes = axes;
    }

    public IReadOnlyList<SurfaceAxisState> Axes { get; }
}

internal sealed class SurfaceAxisState
{
    public SurfaceAxisState(
        string axisKey,
        SurfaceAxisLineGeometry axisLine,
        string titleText,
        Point titlePosition,
        IReadOnlyList<SurfaceAxisTickState> ticks)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(axisKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(titleText);
        ArgumentNullException.ThrowIfNull(ticks);

        AxisKey = axisKey;
        AxisLine = axisLine;
        TitleText = titleText;
        TitlePosition = titlePosition;
        Ticks = ticks;
    }

    public string AxisKey { get; }

    public SurfaceAxisLineGeometry AxisLine { get; }

    public string TitleText { get; }

    public Point TitlePosition { get; }

    public IReadOnlyList<SurfaceAxisTickState> Ticks { get; }
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
