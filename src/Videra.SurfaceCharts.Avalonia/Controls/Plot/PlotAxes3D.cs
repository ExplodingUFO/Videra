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
        Y2 = new PlotAxis3D(owner, PlotAxisKind.Y2);
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
    /// Gets the secondary value axis facade.
    /// </summary>
    public PlotAxis3D Y2 { get; }

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
        Y2.AutoScale();
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
    private PlotAxisLimits? _bounds;

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
            PlotAxisKind.Y2 => _owner.OverlayOptions.SecondaryValueAxisTitleOverride,
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
            PlotAxisKind.Y2 => _owner.OverlayOptions.SecondaryValueAxisUnitOverride,
            _ => _owner.OverlayOptions.DepthAxisUnitOverride,
        };
        set => _owner.SetOverlayOptions(CopyOverlayOptions(_owner.OverlayOptions, label: Label, unit: NormalizeText(value)));
    }

    /// <summary>
    /// Gets or sets the axis scale kind. Only applies to Y and Y2 axes.
    /// </summary>
    public Videra.SurfaceCharts.Core.SurfaceAxisScaleKind ScaleKind
    {
        get => _kind switch
        {
            PlotAxisKind.Y => _owner.OverlayOptions.ValueAxisScaleKind,
            PlotAxisKind.Y2 => _owner.OverlayOptions.SecondaryValueAxisScaleKind,
            _ => Videra.SurfaceCharts.Core.SurfaceAxisScaleKind.Linear,
        };
        set
        {
            if (_kind == PlotAxisKind.Y)
            {
                _owner.SetOverlayOptions(new SurfaceChartOverlayOptions
                {
                    ShowMinorTicks = _owner.OverlayOptions.ShowMinorTicks,
                    MinorTickDivisions = _owner.OverlayOptions.MinorTickDivisions,
                    GridPlane = _owner.OverlayOptions.GridPlane,
                    AxisSideMode = _owner.OverlayOptions.AxisSideMode,
                    TickLabelFormat = _owner.OverlayOptions.TickLabelFormat,
                    TickLabelPrecision = _owner.OverlayOptions.TickLabelPrecision,
                    LegendLabelFormat = _owner.OverlayOptions.LegendLabelFormat,
                    LegendLabelPrecision = _owner.OverlayOptions.LegendLabelPrecision,
                    LabelFormatter = _owner.OverlayOptions.LabelFormatter,
                    XAxisFormatter = _owner.OverlayOptions.XAxisFormatter,
                    YAxisFormatter = _owner.OverlayOptions.YAxisFormatter,
                    ZAxisFormatter = _owner.OverlayOptions.ZAxisFormatter,
                    Y2AxisFormatter = _owner.OverlayOptions.Y2AxisFormatter,
                    HorizontalAxisTitleOverride = _owner.OverlayOptions.HorizontalAxisTitleOverride,
                    HorizontalAxisUnitOverride = _owner.OverlayOptions.HorizontalAxisUnitOverride,
                    ValueAxisTitleOverride = _owner.OverlayOptions.ValueAxisTitleOverride,
                    ValueAxisUnitOverride = _owner.OverlayOptions.ValueAxisUnitOverride,
                    ValueAxisScaleKind = value,
                    SecondaryValueAxisTitleOverride = _owner.OverlayOptions.SecondaryValueAxisTitleOverride,
                    SecondaryValueAxisUnitOverride = _owner.OverlayOptions.SecondaryValueAxisUnitOverride,
                    SecondaryValueAxisScaleKind = _owner.OverlayOptions.SecondaryValueAxisScaleKind,
                    SecondaryValueAxisMinimum = _owner.OverlayOptions.SecondaryValueAxisMinimum,
                    SecondaryValueAxisMaximum = _owner.OverlayOptions.SecondaryValueAxisMaximum,
                    DepthAxisTitleOverride = _owner.OverlayOptions.DepthAxisTitleOverride,
                    DepthAxisUnitOverride = _owner.OverlayOptions.DepthAxisUnitOverride,
                    LegendTitleOverride = _owner.OverlayOptions.LegendTitleOverride,
                    ShowCrosshair = _owner.OverlayOptions.ShowCrosshair,
                    TooltipOffset = _owner.OverlayOptions.TooltipOffset,
                    LegendPosition = _owner.OverlayOptions.LegendPosition,
                });
            }
            else if (_kind == PlotAxisKind.Y2)
            {
                _owner.SetOverlayOptions(new SurfaceChartOverlayOptions
                {
                    ShowMinorTicks = _owner.OverlayOptions.ShowMinorTicks,
                    MinorTickDivisions = _owner.OverlayOptions.MinorTickDivisions,
                    GridPlane = _owner.OverlayOptions.GridPlane,
                    AxisSideMode = _owner.OverlayOptions.AxisSideMode,
                    TickLabelFormat = _owner.OverlayOptions.TickLabelFormat,
                    TickLabelPrecision = _owner.OverlayOptions.TickLabelPrecision,
                    LegendLabelFormat = _owner.OverlayOptions.LegendLabelFormat,
                    LegendLabelPrecision = _owner.OverlayOptions.LegendLabelPrecision,
                    LabelFormatter = _owner.OverlayOptions.LabelFormatter,
                    XAxisFormatter = _owner.OverlayOptions.XAxisFormatter,
                    YAxisFormatter = _owner.OverlayOptions.YAxisFormatter,
                    ZAxisFormatter = _owner.OverlayOptions.ZAxisFormatter,
                    Y2AxisFormatter = _owner.OverlayOptions.Y2AxisFormatter,
                    HorizontalAxisTitleOverride = _owner.OverlayOptions.HorizontalAxisTitleOverride,
                    HorizontalAxisUnitOverride = _owner.OverlayOptions.HorizontalAxisUnitOverride,
                    ValueAxisTitleOverride = _owner.OverlayOptions.ValueAxisTitleOverride,
                    ValueAxisUnitOverride = _owner.OverlayOptions.ValueAxisUnitOverride,
                    ValueAxisScaleKind = _owner.OverlayOptions.ValueAxisScaleKind,
                    SecondaryValueAxisTitleOverride = _owner.OverlayOptions.SecondaryValueAxisTitleOverride,
                    SecondaryValueAxisUnitOverride = _owner.OverlayOptions.SecondaryValueAxisUnitOverride,
                    SecondaryValueAxisScaleKind = value,
                    SecondaryValueAxisMinimum = _owner.OverlayOptions.SecondaryValueAxisMinimum,
                    SecondaryValueAxisMaximum = _owner.OverlayOptions.SecondaryValueAxisMaximum,
                    DepthAxisTitleOverride = _owner.OverlayOptions.DepthAxisTitleOverride,
                    DepthAxisUnitOverride = _owner.OverlayOptions.DepthAxisUnitOverride,
                    LegendTitleOverride = _owner.OverlayOptions.LegendTitleOverride,
                    ShowCrosshair = _owner.OverlayOptions.ShowCrosshair,
                    TooltipOffset = _owner.OverlayOptions.TooltipOffset,
                    LegendPosition = _owner.OverlayOptions.LegendPosition,
                });
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this axis preserves its current limits during Plot.Axes limit and autoscale calls.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets the explicit minimum/maximum bounds applied to Plot.Axes limit and autoscale calls.
    /// </summary>
    public PlotAxisLimits? Bounds => _bounds;

    /// <summary>
    /// Sets explicit bounds used to clamp Plot.Axes limit and autoscale calls for this axis.
    /// </summary>
    /// <param name="minimum">The optional inclusive lower bound.</param>
    /// <param name="maximum">The optional inclusive upper bound.</param>
    public void SetBounds(double? minimum, double? maximum)
    {
        ValidateOptionalLimit(minimum, nameof(minimum));
        ValidateOptionalLimit(maximum, nameof(maximum));

        if (minimum is { } lower && maximum is { } upper && upper <= lower)
        {
            throw new ArgumentException("Axis maximum bound must be greater than axis minimum bound.", nameof(maximum));
        }

        _bounds = new PlotAxisLimits(minimum ?? double.NegativeInfinity, maximum ?? double.PositiveInfinity);
    }

    /// <summary>
    /// Clears explicit bounds for this axis.
    /// </summary>
    public void ClearBounds()
    {
        _bounds = null;
    }

    /// <summary>
    /// Sets the visible limits for this axis.
    /// </summary>
    /// <param name="minimum">The inclusive minimum value.</param>
    /// <param name="maximum">The inclusive maximum value.</param>
    public void SetLimits(double minimum, double maximum)
    {
        ValidateLimits(minimum, maximum);
        if (IsLocked)
        {
            return;
        }

        var limits = ApplyBounds(new PlotAxisLimits(minimum, maximum));

        switch (_kind)
        {
            case PlotAxisKind.X:
                _owner.SetHorizontalAxisLimits(limits.Minimum, limits.Maximum);
                break;
            case PlotAxisKind.Y:
                _owner.SetValueAxisLimits(limits.Minimum, limits.Maximum);
                break;
            case PlotAxisKind.Y2:
                _owner.SetOverlayOptions(new SurfaceChartOverlayOptions
                {
                    ShowMinorTicks = _owner.OverlayOptions.ShowMinorTicks,
                    MinorTickDivisions = _owner.OverlayOptions.MinorTickDivisions,
                    GridPlane = _owner.OverlayOptions.GridPlane,
                    AxisSideMode = _owner.OverlayOptions.AxisSideMode,
                    TickLabelFormat = _owner.OverlayOptions.TickLabelFormat,
                    TickLabelPrecision = _owner.OverlayOptions.TickLabelPrecision,
                    LegendLabelFormat = _owner.OverlayOptions.LegendLabelFormat,
                    LegendLabelPrecision = _owner.OverlayOptions.LegendLabelPrecision,
                    LabelFormatter = _owner.OverlayOptions.LabelFormatter,
                    XAxisFormatter = _owner.OverlayOptions.XAxisFormatter,
                    YAxisFormatter = _owner.OverlayOptions.YAxisFormatter,
                    ZAxisFormatter = _owner.OverlayOptions.ZAxisFormatter,
                    Y2AxisFormatter = _owner.OverlayOptions.Y2AxisFormatter,
                    HorizontalAxisTitleOverride = _owner.OverlayOptions.HorizontalAxisTitleOverride,
                    HorizontalAxisUnitOverride = _owner.OverlayOptions.HorizontalAxisUnitOverride,
                    ValueAxisTitleOverride = _owner.OverlayOptions.ValueAxisTitleOverride,
                    ValueAxisUnitOverride = _owner.OverlayOptions.ValueAxisUnitOverride,
                    ValueAxisScaleKind = _owner.OverlayOptions.ValueAxisScaleKind,
                    SecondaryValueAxisTitleOverride = _owner.OverlayOptions.SecondaryValueAxisTitleOverride,
                    SecondaryValueAxisUnitOverride = _owner.OverlayOptions.SecondaryValueAxisUnitOverride,
                    SecondaryValueAxisScaleKind = _owner.OverlayOptions.SecondaryValueAxisScaleKind,
                    SecondaryValueAxisMinimum = limits.Minimum,
                    SecondaryValueAxisMaximum = limits.Maximum,
                    DepthAxisTitleOverride = _owner.OverlayOptions.DepthAxisTitleOverride,
                    DepthAxisUnitOverride = _owner.OverlayOptions.DepthAxisUnitOverride,
                    LegendTitleOverride = _owner.OverlayOptions.LegendTitleOverride,
                    ShowCrosshair = _owner.OverlayOptions.ShowCrosshair,
                    TooltipOffset = _owner.OverlayOptions.TooltipOffset,
                    LegendPosition = _owner.OverlayOptions.LegendPosition,
                });
                break;
            case PlotAxisKind.Z:
                _owner.SetDepthAxisLimits(limits.Minimum, limits.Maximum);
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
            PlotAxisKind.Y2 => GetSecondaryValueAxisLimits(),
            _ => _owner.GetDepthAxisLimits(),
        };
    }

    private PlotAxisLimits? GetSecondaryValueAxisLimits()
    {
        var opts = _owner.OverlayOptions;
        if (opts.SecondaryValueAxisMinimum is { } min && opts.SecondaryValueAxisMaximum is { } max)
        {
            return new PlotAxisLimits(min, max);
        }

        return null;
    }

    /// <summary>
    /// Resets this axis limits to the active series metadata.
    /// </summary>
    public void AutoScale()
    {
        if (IsLocked)
        {
            return;
        }

        var limits = _kind switch
        {
            PlotAxisKind.X => _owner.GetNaturalHorizontalAxisLimits(),
            PlotAxisKind.Y => _owner.GetNaturalValueAxisLimits(),
            PlotAxisKind.Y2 => null, // Y2 has no natural data range
            _ => _owner.GetNaturalDepthAxisLimits(),
        };

        if (limits is { } naturalLimits)
        {
            var boundedLimits = ApplyBounds(naturalLimits);
            SetLimits(boundedLimits.Minimum, boundedLimits.Maximum);
        }
    }

    private PlotAxisLimits ApplyBounds(PlotAxisLimits limits)
    {
        if (_bounds is not { } bounds)
        {
            return limits;
        }

        var minimum = Math.Max(limits.Minimum, bounds.Minimum);
        var maximum = Math.Min(limits.Maximum, bounds.Maximum);
        if (maximum <= minimum)
        {
            throw new InvalidOperationException("Axis bounds leave no visible limit span.");
        }

        return new PlotAxisLimits(minimum, maximum);
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
            Y2AxisFormatter = source.Y2AxisFormatter,
            HorizontalAxisTitleOverride = _kind == PlotAxisKind.X ? label : source.HorizontalAxisTitleOverride,
            HorizontalAxisUnitOverride = _kind == PlotAxisKind.X ? unit : source.HorizontalAxisUnitOverride,
            ValueAxisTitleOverride = _kind == PlotAxisKind.Y ? label : source.ValueAxisTitleOverride,
            ValueAxisUnitOverride = _kind == PlotAxisKind.Y ? unit : source.ValueAxisUnitOverride,
            ValueAxisScaleKind = source.ValueAxisScaleKind,
            SecondaryValueAxisTitleOverride = _kind == PlotAxisKind.Y2 ? label : source.SecondaryValueAxisTitleOverride,
            SecondaryValueAxisUnitOverride = _kind == PlotAxisKind.Y2 ? unit : source.SecondaryValueAxisUnitOverride,
            SecondaryValueAxisScaleKind = source.SecondaryValueAxisScaleKind,
            SecondaryValueAxisMinimum = source.SecondaryValueAxisMinimum,
            SecondaryValueAxisMaximum = source.SecondaryValueAxisMaximum,
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

    private static void ValidateOptionalLimit(double? value, string paramName)
    {
        if (value is { } actualValue && !double.IsFinite(actualValue))
        {
            throw new ArgumentOutOfRangeException(paramName, "Axis bounds must be finite when specified.");
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
    Y2,
    Z,
}
