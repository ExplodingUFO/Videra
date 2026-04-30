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
        return Create(status, activeRecipeContext, null);
    }

    /// <summary>
    /// Creates a bounded text block describing the workspace state, including linked interaction details.
    /// </summary>
    /// <param name="status">The workspace status snapshot.</param>
    /// <param name="activeRecipeContext">The recipe context of the active chart, or null.</param>
    /// <param name="linkedInteractionStates">Optional linked interaction states for each registered link group.</param>
    /// <returns>A bounded diagnostic text block.</returns>
    public static string Create(
        SurfaceChartWorkspaceStatus status,
        string? activeRecipeContext,
        IReadOnlyList<SurfaceChartLinkedInteractionState>? linkedInteractionStates)
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

        if (linkedInteractionStates is { Count: > 0 })
        {
            for (var i = 0; i < linkedInteractionStates.Count; i++)
            {
                var state = linkedInteractionStates[i];
                var surfaces = new List<string>();
                if (state.PropagateSelection) surfaces.Add("Selection");
                if (state.PropagateProbe) surfaces.Add("Probe");
                if (state.PropagateMeasurement) surfaces.Add("Measurement");
                var surfacesText = surfaces.Count > 0 ? string.Join(", ", surfaces) : "none";

                sb.AppendLine(
                    $"LinkGroup[{i}]: Policy={state.Policy} | Members={state.MemberCount} | Propagation={surfacesText}");
            }

            var anySelection = linkedInteractionStates.Any(s => s.PropagateSelection);
            var anyProbe = linkedInteractionStates.Any(s => s.PropagateProbe);
            var anyMeasurement = linkedInteractionStates.Any(s => s.PropagateMeasurement);

            sb.AppendLine(
                $"InteractionSurfaces: Selection={(anySelection ? "active" : "inactive")} | " +
                $"Probe={(anyProbe ? "active" : "inactive")} | " +
                $"Measurement={(anyMeasurement ? "active" : "inactive")}");

            sb.AppendLine("EvidenceBoundary: Propagation is runtime truth; linked chart data presence is runtime truth.");
        }

        sb.Append($"AllReady: {status.AllReady}");

        return sb.ToString();
    }
}
