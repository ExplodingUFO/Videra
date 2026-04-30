using System.Text;

namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Bounded text evidence formatter for <see cref="SurfaceChartWorkspaceStatus"/>.
/// </summary>
/// <remarks>
/// Produces a support-readable text block that a consumer can copy for diagnostics.
/// This is NOT a workbench export or report generator — it is a bounded diagnostic text.
/// </remarks>
public static class SurfaceChartWorkspaceEvidence
{
    /// <summary>
    /// Creates a bounded text block describing the workspace state.
    /// </summary>
    /// <param name="status">The workspace status snapshot.</param>
    /// <param name="activeRecipeContext">The recipe context of the active chart, or null.</param>
    /// <returns>A bounded diagnostic text block.</returns>
    public static string Create(SurfaceChartWorkspaceStatus status, string? activeRecipeContext)
    {
        ArgumentNullException.ThrowIfNull(status);

        var sb = new StringBuilder();
        sb.AppendLine("SurfaceCharts workspace evidence");
        sb.AppendLine($"GeneratedUtc: {DateTimeOffset.UtcNow:O}");
        sb.AppendLine($"ChartCount: {status.ChartCount}");
        sb.AppendLine($"ActiveChartId: {status.ActiveChartId ?? "none"}");
        sb.AppendLine($"ActiveRecipeContext: {activeRecipeContext ?? "none"}");

        foreach (var panel in status.Panels)
        {
            sb.AppendLine(
                $"Panel: {panel.ChartId} | {panel.Label} | {panel.ChartKind} | Ready={panel.IsReady} | Series={panel.SeriesCount} | Points={panel.PointCount}");
        }

        sb.AppendLine($"LinkGroupCount: {status.LinkGroupCount}");
        sb.Append($"AllReady: {status.AllReady}");

        return sb.ToString();
    }
}
