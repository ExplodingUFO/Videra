namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Describes the active interaction surfaces for a single link group.
/// </summary>
public sealed record SurfaceChartLinkedInteractionState
{
    /// <summary>
    /// Gets the link policy for the group.
    /// </summary>
    public required SurfaceChartLinkPolicy Policy { get; init; }

    /// <summary>
    /// Gets the number of member charts in the group.
    /// </summary>
    public required int MemberCount { get; init; }

    /// <summary>
    /// Gets whether selection propagation is enabled for this group.
    /// </summary>
    public required bool PropagateSelection { get; init; }

    /// <summary>
    /// Gets whether probe propagation is enabled for this group.
    /// </summary>
    public required bool PropagateProbe { get; init; }

    /// <summary>
    /// Gets whether measurement propagation is enabled for this group.
    /// </summary>
    public required bool PropagateMeasurement { get; init; }
}
