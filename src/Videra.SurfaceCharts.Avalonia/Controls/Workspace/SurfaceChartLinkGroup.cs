namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Link policy for a <see cref="SurfaceChartLinkGroup"/>.
/// </summary>
public enum SurfaceChartLinkPolicy
{
    /// <summary>
    /// Synchronize the full view state (camera, data window, axes) across linked charts.
    /// </summary>
    FullViewState,

    /// <summary>
    /// Synchronize only the camera pose across linked charts. Not yet supported in Phase 426.
    /// </summary>
    CameraOnly,

    /// <summary>
    /// Synchronize only the axis limits across linked charts. Not yet supported in Phase 426.
    /// </summary>
    AxisOnly,
}

/// <summary>
/// Manages a group of <see cref="VideraChartView"/> instances linked via pairwise view-state synchronization.
/// </summary>
/// <remarks>
/// Disposing the group unlinks all members. A chart can be in at most one link group at a time
/// (enforced by the host, not the group itself).
/// </remarks>
public sealed class SurfaceChartLinkGroup : IDisposable
{
    private readonly List<VideraChartView> _members = [];
    private readonly List<IDisposable> _pairwiseLinks = [];
    private readonly SurfaceChartLinkPolicy _policy;
    private bool _disposed;

    /// <summary>
    /// Initializes a new link group with the specified policy.
    /// </summary>
    /// <param name="policy">The link policy. Only <see cref="SurfaceChartLinkPolicy.FullViewState"/> is supported in Phase 426.</param>
    /// <exception cref="NotSupportedException">The policy is not <see cref="SurfaceChartLinkPolicy.FullViewState"/>.</exception>
    public SurfaceChartLinkGroup(SurfaceChartLinkPolicy policy = SurfaceChartLinkPolicy.FullViewState)
    {
        _policy = policy;
    }

    /// <summary>
    /// Gets the current member charts in this link group.
    /// </summary>
    public IReadOnlyList<VideraChartView> Members => _members.AsReadOnly();

    /// <summary>
    /// Adds a chart to the group, creating pairwise links to all existing members.
    /// </summary>
    /// <param name="chart">The chart view to add.</param>
    /// <exception cref="ObjectDisposedException">The group has been disposed.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="chart"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The chart is already a member of this group.</exception>
    public void Add(VideraChartView chart)
    {
        throw new NotImplementedException("Stub for RED phase");
    }

    /// <summary>
    /// Removes a chart from the group, disposing all pairwise links involving it.
    /// </summary>
    /// <param name="chart">The chart view to remove.</param>
    public void Remove(VideraChartView chart)
    {
        throw new NotImplementedException("Stub for RED phase");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        throw new NotImplementedException("Stub for RED phase");
    }
}
