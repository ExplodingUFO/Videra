using System.Globalization;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes chart-local axis, grid, and legend presentation options.
/// </summary>
public sealed class SurfaceChartOverlayOptions
{
    /// <summary>
    /// Gets the immutable default overlay options.
    /// </summary>
    public static SurfaceChartOverlayOptions Default { get; } = new();

    /// <summary>
    /// Gets a value indicating whether minor ticks are rendered alongside major ticks.
    /// </summary>
    public bool ShowMinorTicks { get; init; }

    /// <summary>
    /// Gets the number of subdivisions inserted between neighboring major ticks when minor ticks are enabled.
    /// </summary>
    public int MinorTickDivisions { get; init; } = 4;

    /// <summary>
    /// Gets the grid plane selection policy for the projected overlay grid.
    /// </summary>
    public SurfaceChartGridPlane GridPlane { get; init; } = SurfaceChartGridPlane.Auto;

    /// <summary>
    /// Gets the axis-side selection policy for the visible overlay corner.
    /// </summary>
    public SurfaceChartAxisSideMode AxisSideMode { get; init; } = SurfaceChartAxisSideMode.Auto;

    /// <summary>
    /// Gets the numeric formatting strategy for axis tick labels.
    /// </summary>
    public SurfaceChartNumericLabelFormat TickLabelFormat { get; init; } = SurfaceChartNumericLabelFormat.General;

    /// <summary>
    /// Gets the numeric precision for axis tick labels.
    /// </summary>
    public int TickLabelPrecision { get; init; } = 3;

    /// <summary>
    /// Gets the numeric formatting strategy for legend labels.
    /// </summary>
    public SurfaceChartNumericLabelFormat LegendLabelFormat { get; init; } = SurfaceChartNumericLabelFormat.General;

    /// <summary>
    /// Gets the numeric precision for legend labels.
    /// </summary>
    public int LegendLabelPrecision { get; init; } = 3;

    /// <summary>
    /// Gets the optional chart-local label formatter. The formatter receives an axis key such as
    /// <c>X</c>, <c>Y</c>, <c>Z</c>, or <c>Legend</c>.
    /// </summary>
    public Func<string, double, string>? LabelFormatter { get; init; }

    /// <summary>
    /// Gets the optional horizontal-axis title override.
    /// </summary>
    public string? HorizontalAxisTitleOverride { get; init; }

    /// <summary>
    /// Gets the optional horizontal-axis unit override.
    /// </summary>
    public string? HorizontalAxisUnitOverride { get; init; }

    /// <summary>
    /// Gets the optional value-axis title override.
    /// </summary>
    public string? ValueAxisTitleOverride { get; init; }

    /// <summary>
    /// Gets the optional value-axis unit override.
    /// </summary>
    public string? ValueAxisUnitOverride { get; init; }

    /// <summary>
    /// Gets the optional depth-axis title override.
    /// </summary>
    public string? DepthAxisTitleOverride { get; init; }

    /// <summary>
    /// Gets the optional depth-axis unit override.
    /// </summary>
    public string? DepthAxisUnitOverride { get; init; }

    /// <summary>
    /// Gets the optional legend title override.
    /// </summary>
    public string? LegendTitleOverride { get; init; }

    /// <summary>
    /// Formats a label for the specified axis key and numeric value.
    /// </summary>
    /// <param name="axisKey">The axis or legend key.</param>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted label text.</returns>
    public string FormatLabel(string axisKey, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(axisKey);

        if (LabelFormatter is not null)
        {
            return LabelFormatter(axisKey, value);
        }

        if (string.Equals(axisKey, "Legend", StringComparison.Ordinal))
        {
            return FormatNumericLabel(value, LegendLabelFormat, LegendLabelPrecision);
        }

        return FormatNumericLabel(value, TickLabelFormat, TickLabelPrecision);
    }

    internal string FormatProbeAxisX(double value) => FormatLabel("X", value);

    internal string FormatProbeAxisY(double value) => FormatLabel("Z", value);

    internal string FormatProbeValue(double value) => FormatLabel("Y", value);

    internal string FormatProbeDelta(string axisKey, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(axisKey);

        var formattedValue = FormatLabel(axisKey, value);
        return value > 0d ? $"+{formattedValue}" : formattedValue;
    }

    internal static string FormatNumericLabel(double value, SurfaceChartNumericLabelFormat format, int precision)
    {
        return format switch
        {
            SurfaceChartNumericLabelFormat.Engineering => FormatEngineering(value, precision),
            SurfaceChartNumericLabelFormat.Scientific => FormatScientific(value, precision),
            SurfaceChartNumericLabelFormat.Fixed => FormatFixed(value, precision),
            _ => FormatGeneral(value, precision),
        };
    }

    private static string FormatGeneral(double value, int precision)
    {
        var safePrecision = NormalizePrecision(precision);
        return value.ToString(safePrecision == 0 ? "0" : $"0.{new string('#', safePrecision)}", CultureInfo.InvariantCulture);
    }

    private static string FormatScientific(double value, int precision)
    {
        var safePrecision = NormalizePrecision(precision);
        var format = safePrecision == 0 ? "0E+0" : $"0.{new string('0', safePrecision)}E+0";

        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    private static string FormatEngineering(double value, int precision)
    {
        if (!double.IsFinite(value))
        {
            return value.ToString("G", CultureInfo.InvariantCulture);
        }

        if (Math.Abs(value) <= double.Epsilon)
        {
            return FormatFixed(0d, precision);
        }

        var absValue = Math.Abs(value);
        var exponent = (int)(Math.Floor(Math.Log10(absValue) / 3d) * 3d);
        var scaled = value / Math.Pow(10d, exponent);

        return $"{FormatFixed(scaled, precision)}E{(exponent >= 0 ? "+" : "")}{exponent}";
    }

    private static string FormatFixed(double value, int precision)
    {
        return value.ToString($"F{NormalizePrecision(precision)}", CultureInfo.InvariantCulture);
    }

    private static int NormalizePrecision(int precision)
    {
        return Math.Clamp(precision, 0, 12);
    }
}

/// <summary>
/// Selects the numeric formatting style used by overlay labels.
/// </summary>
public enum SurfaceChartNumericLabelFormat
{
    /// <summary>Auto-style labels: trims trailing zeros and keeps a small precision.</summary>
    General,

    /// <summary>Engineering-style exponent notation with 10^3 aligned exponents.</summary>
    Engineering,

    /// <summary>Scientific notation (1.234E+03 style).</summary>
    Scientific,

    /// <summary>Fixed-point notation.</summary>
    Fixed,
}

/// <summary>
/// Reusable numeric-label presets for common chart-local presentation profiles.
/// </summary>
public static class SurfaceChartNumericLabelPresets
{
    /// <summary>
    /// Creates an engineering format overlay preset that aligns exponents by 3.
    /// </summary>
    /// <param name="precision">The number of fixed fractional digits for each scaled mantissa.</param>
    /// <returns>An overlay option preset using engineering numeric labels.</returns>
    public static SurfaceChartOverlayOptions Engineering(int precision = 3)
        => new()
        {
            TickLabelFormat = SurfaceChartNumericLabelFormat.Engineering,
            TickLabelPrecision = precision,
            LegendLabelFormat = SurfaceChartNumericLabelFormat.Engineering,
            LegendLabelPrecision = precision,
        };

    /// <summary>
    /// Creates a scientific format overlay preset.
    /// </summary>
    /// <param name="precision">The number of fixed fractional digits in scientific notation.</param>
    /// <returns>An overlay option preset using scientific numeric labels.</returns>
    public static SurfaceChartOverlayOptions Scientific(int precision = 3)
        => new()
        {
            TickLabelFormat = SurfaceChartNumericLabelFormat.Scientific,
            TickLabelPrecision = precision,
            LegendLabelFormat = SurfaceChartNumericLabelFormat.Scientific,
            LegendLabelPrecision = precision,
        };

    /// <summary>
    /// Creates a fixed-point overlay preset.
    /// </summary>
    /// <param name="precision">The number of fixed fractional digits.</param>
    /// <returns>An overlay option preset using fixed-point numeric labels.</returns>
    public static SurfaceChartOverlayOptions Fixed(int precision = 3)
        => new()
        {
            TickLabelFormat = SurfaceChartNumericLabelFormat.Fixed,
            TickLabelPrecision = precision,
            LegendLabelFormat = SurfaceChartNumericLabelFormat.Fixed,
            LegendLabelPrecision = precision,
        };
}

/// <summary>
/// Reusable chart-local presentation presets.
/// </summary>
public static class SurfaceChartOverlayPresets
{
    /// <summary>
    /// Balanced presentation preset with minor ticks enabled and scientific labels.
    /// </summary>
    public static SurfaceChartOverlayOptions Professional { get; } = new()
    {
        ShowMinorTicks = true,
        MinorTickDivisions = 4,
        TickLabelFormat = SurfaceChartNumericLabelFormat.Scientific,
        TickLabelPrecision = 3,
        LegendLabelFormat = SurfaceChartNumericLabelFormat.Scientific,
        LegendLabelPrecision = 3,
        GridPlane = SurfaceChartGridPlane.XZ,
        AxisSideMode = SurfaceChartAxisSideMode.Auto,
    };

    /// <summary>
    /// Compact presentation preset used for compact dashboards with fewer tick decorations.
    /// </summary>
    public static SurfaceChartOverlayOptions Compact { get; } = new()
    {
        ShowMinorTicks = false,
        MinorTickDivisions = 2,
        TickLabelFormat = SurfaceChartNumericLabelFormat.General,
        TickLabelPrecision = 3,
        LegendLabelFormat = SurfaceChartNumericLabelFormat.General,
        LegendLabelPrecision = 3,
        GridPlane = SurfaceChartGridPlane.XZ,
        AxisSideMode = SurfaceChartAxisSideMode.Auto,
    };
}

/// <summary>
/// Selects which projected axis and label the chart uses for grid lines.
/// </summary>
public enum SurfaceChartGridPlane
{
    /// <summary>
    /// Lets the chart choose its default projected grid plane.
    /// </summary>
    Auto,

    /// <summary>
    /// Uses the horizontal/value plane.
    /// </summary>
    XY,

    /// <summary>
    /// Uses the horizontal/depth plane.
    /// </summary>
    XZ,

    /// <summary>
    /// Uses the value/depth plane.
    /// </summary>
    YZ,
}

/// <summary>
/// Selects which projected plot corner hosts the visible axes.
/// </summary>
public enum SurfaceChartAxisSideMode
{
    /// <summary>
    /// Lets the chart choose the visible overlay corner automatically.
    /// </summary>
    Auto,

    /// <summary>
    /// Pins the visible overlay corner to the minimum horizontal and depth bounds.
    /// </summary>
    MinimumBounds,

    /// <summary>
    /// Pins the visible overlay corner to the maximum horizontal and depth bounds.
    /// </summary>
    MaximumBounds,
}

