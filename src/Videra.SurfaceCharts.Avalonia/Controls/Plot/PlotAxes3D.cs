using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Provides convenient access to the plot's chart-local X, Y, and Z axes.
/// </summary>
public sealed class PlotAxes3D
{
    internal PlotAxes3D(Plot3D owner)
    {
        X = new PlotAxis3D(owner, PlotAxisKind.X);
        Y = new PlotAxis3D(owner, PlotAxisKind.Y);
        Z = new PlotAxis3D(owner, PlotAxisKind.Z);
    }

    /// <summary>
    /// Gets the horizontal axis facade.
    /// </summary>
    public PlotAxis3D X { get; }

    /// <summary>
    /// Gets the value axis facade.
    /// </summary>
    public PlotAxis3D Y { get; }

    /// <summary>
    /// Gets the depth axis facade.
    /// </summary>
    public PlotAxis3D Z { get; }

    /// <summary>
    /// Resets all axis limits to the active series metadata.
    /// </summary>
    public void AutoScale()
    {
        X.AutoScale();
        Y.AutoScale();
        Z.AutoScale();
    }
}

/// <summary>
/// Provides convenient label, unit, and limit access for one plot axis.
/// </summary>
public sealed class PlotAxis3D
{
    private readonly Plot3D _owner;
    private readonly PlotAxisKind _kind;

    internal PlotAxis3D(Plot3D owner, PlotAxisKind kind)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _kind = kind;
    }

    /// <summary>
    /// Gets or sets the axis title override.
    /// </summary>
    public string? Label
    {
        get => _kind switch
        {
            PlotAxisKind.X => _owner.OverlayOptions.HorizontalAxisTitleOverride,
            PlotAxisKind.Y => _owner.OverlayOptions.ValueAxisTitleOverride,
            _ => _owner.OverlayOptions.DepthAxisTitleOverride,
        };
        set => _owner.SetOverlayOptions(CopyOverlayOptions(_owner.OverlayOptions, label: NormalizeText(value), unit: Unit));
    }

    /// <summary>
    /// Gets or sets the axis unit override.
    /// </summary>
    public string? Unit
    {
        get => _kind switch
        {
            PlotAxisKind.X => _owner.OverlayOptions.HorizontalAxisUnitOverride,
            PlotAxisKind.Y => _owner.OverlayOptions.ValueAxisUnitOverride,
            _ => _owner.OverlayOptions.DepthAxisUnitOverride,
        };
        set => _owner.SetOverlayOptions(CopyOverlayOptions(_owner.OverlayOptions, label: Label, unit: NormalizeText(value)));
    }

    /// <summary>
    /// Sets the visible limits for this axis.
    /// </summary>
    /// <param name="minimum">The inclusive minimum value.</param>
    /// <param name="maximum">The inclusive maximum value.</param>
    public void SetLimits(double minimum, double maximum)
    {
        ValidateLimits(minimum, maximum);

        switch (_kind)
        {
            case PlotAxisKind.X:
                _owner.SetHorizontalAxisLimits(minimum, maximum);
                break;
            case PlotAxisKind.Y:
                _owner.SetValueAxisLimits(minimum, maximum);
                break;
            case PlotAxisKind.Z:
                _owner.SetDepthAxisLimits(minimum, maximum);
                break;
        }
    }

    /// <summary>
    /// Gets the current limits for this axis, or <c>null</c> when no axis limits are available.
    /// </summary>
    public PlotAxisLimits? GetLimits()
    {
        return _kind switch
        {
            PlotAxisKind.X => _owner.GetHorizontalAxisLimits(),
            PlotAxisKind.Y => _owner.GetValueAxisLimits(),
            _ => _owner.GetDepthAxisLimits(),
        };
    }

    /// <summary>
    /// Resets this axis limits to the active series metadata.
    /// </summary>
    public void AutoScale()
    {
        switch (_kind)
        {
            case PlotAxisKind.X:
                _owner.AutoScaleHorizontalAxis();
                break;
            case PlotAxisKind.Y:
                _owner.AutoScaleValueAxis();
                break;
            case PlotAxisKind.Z:
                _owner.AutoScaleDepthAxis();
                break;
        }
    }

    private SurfaceChartOverlayOptions CopyOverlayOptions(SurfaceChartOverlayOptions source, string? label, string? unit)
    {
        return new SurfaceChartOverlayOptions
        {
            ShowMinorTicks = source.ShowMinorTicks,
            MinorTickDivisions = source.MinorTickDivisions,
            GridPlane = source.GridPlane,
            AxisSideMode = source.AxisSideMode,
            TickLabelFormat = source.TickLabelFormat,
            TickLabelPrecision = source.TickLabelPrecision,
            LegendLabelFormat = source.LegendLabelFormat,
            LegendLabelPrecision = source.LegendLabelPrecision,
            LabelFormatter = source.LabelFormatter,
            XAxisFormatter = source.XAxisFormatter,
            YAxisFormatter = source.YAxisFormatter,
            ZAxisFormatter = source.ZAxisFormatter,
            HorizontalAxisTitleOverride = _kind == PlotAxisKind.X ? label : source.HorizontalAxisTitleOverride,
            HorizontalAxisUnitOverride = _kind == PlotAxisKind.X ? unit : source.HorizontalAxisUnitOverride,
            ValueAxisTitleOverride = _kind == PlotAxisKind.Y ? label : source.ValueAxisTitleOverride,
            ValueAxisUnitOverride = _kind == PlotAxisKind.Y ? unit : source.ValueAxisUnitOverride,
            DepthAxisTitleOverride = _kind == PlotAxisKind.Z ? label : source.DepthAxisTitleOverride,
            DepthAxisUnitOverride = _kind == PlotAxisKind.Z ? unit : source.DepthAxisUnitOverride,
            LegendTitleOverride = source.LegendTitleOverride,
            ShowCrosshair = source.ShowCrosshair,
            TooltipOffset = source.TooltipOffset,
            LegendPosition = source.LegendPosition,
        };
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void ValidateLimits(double minimum, double maximum)
    {
        if (!double.IsFinite(minimum))
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), "Axis minimum must be finite.");
        }

        if (!double.IsFinite(maximum))
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "Axis maximum must be finite.");
        }

        if (maximum <= minimum)
        {
            throw new ArgumentException("Axis maximum must be greater than axis minimum.", nameof(maximum));
        }
    }
}

/// <summary>
/// Represents an axis minimum/maximum pair.
/// </summary>
/// <param name="Minimum">The inclusive axis minimum.</param>
/// <param name="Maximum">The inclusive axis maximum.</param>
public readonly record struct PlotAxisLimits(double Minimum, double Maximum);

internal enum PlotAxisKind
{
    X,
    Y,
    Z,
}
