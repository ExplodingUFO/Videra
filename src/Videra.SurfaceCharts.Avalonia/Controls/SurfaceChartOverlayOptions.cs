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

        return LabelFormatter?.Invoke(axisKey, value)
            ?? value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// Selects which projected plot plane receives grid lines.
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
