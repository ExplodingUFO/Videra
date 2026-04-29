using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness state for contour chart rendering.
/// </summary>
public sealed record ContourChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has a contour source.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render contour lines.
    /// </summary>
    public bool IsReady { get; init; }

    /// <summary>
    /// Gets the number of contour levels.
    /// </summary>
    public int LevelCount { get; init; }

    /// <summary>
    /// Gets the number of extracted contour lines.
    /// </summary>
    public int ExtractedLineCount { get; init; }

    /// <summary>
    /// Gets the total number of contour segments across all lines.
    /// </summary>
    public int TotalSegmentCount { get; init; }
}
