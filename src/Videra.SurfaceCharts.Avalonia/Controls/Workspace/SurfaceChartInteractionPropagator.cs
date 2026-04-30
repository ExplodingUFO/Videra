using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Host-owned propagator that forwards probe, selection, and measurement interactions
/// across members of a <see cref="SurfaceChartLinkGroup"/>.
/// </summary>
/// <remarks>
/// <para>
/// Propagation is opt-in: the host creates a propagator with explicit flags for which
/// interaction surfaces to propagate. The propagator subscribes to relevant events on all
/// link group members and forwards interactions to non-sender members.
/// </para>
/// <para>
/// A re-entrancy guard prevents infinite propagation loops when linked charts fire events
/// back through the propagator.
/// </para>
/// </remarks>
public sealed class SurfaceChartInteractionPropagator : IDisposable
{
    private readonly SurfaceChartLinkGroup _linkGroup;
    private readonly bool _propagateSelection;
    private readonly bool _propagateProbe;
    private readonly bool _propagateMeasurement;
    private bool _isPropagating;
    private bool _disposed;

    /// <summary>
    /// Initializes a new propagator for the specified link group.
    /// </summary>
    /// <param name="linkGroup">The link group whose members will receive propagated interactions.</param>
    /// <param name="propagateSelection">Whether to propagate selection reports across members.</param>
    /// <param name="propagateProbe">Whether to propagate probe resolution across members.</param>
    /// <param name="propagateMeasurement">Whether to propagate measurement reports across members.</param>
    /// <exception cref="ArgumentNullException"><paramref name="linkGroup"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="linkGroup"/> has no members.</exception>
    public SurfaceChartInteractionPropagator(
        SurfaceChartLinkGroup linkGroup,
        bool propagateSelection = false,
        bool propagateProbe = false,
        bool propagateMeasurement = false)
    {
        ArgumentNullException.ThrowIfNull(linkGroup);

        if (linkGroup.Members.Count == 0)
        {
            throw new ArgumentException("Link group must have at least one member.", nameof(linkGroup));
        }

        _linkGroup = linkGroup;
        _propagateSelection = propagateSelection;
        _propagateProbe = propagateProbe;
        _propagateMeasurement = propagateMeasurement;

        if (_propagateSelection || _propagateMeasurement)
        {
            foreach (var member in _linkGroup.Members)
            {
                member.SelectionReported += OnSelectionReported;
            }
        }
    }

    /// <summary>
    /// Propagates a probe from the source chart to all other members of the link group.
    /// </summary>
    /// <param name="sourceChart">The chart where the probe originated.</param>
    /// <param name="screenPosition">The screen position of the probe.</param>
    /// <returns><c>true</c> when propagation was performed; otherwise, <c>false</c>.</returns>
    public bool PropagateProbe(VideraChartView sourceChart, Point screenPosition)
    {
        if (_isPropagating || !_propagateProbe || _disposed)
        {
            return false;
        }

        _isPropagating = true;
        try
        {
            foreach (var member in _linkGroup.Members)
            {
                if (!ReferenceEquals(member, sourceChart))
                {
                    member.TryResolveProbe(screenPosition, out _);
                }
            }
        }
        finally
        {
            _isPropagating = false;
        }

        return true;
    }

    /// <summary>
    /// Captures a snapshot of this propagator's interaction configuration.
    /// </summary>
    /// <returns>A record describing the active interaction surfaces.</returns>
    public SurfaceChartLinkedInteractionState CaptureInteractionState()
    {
        return new SurfaceChartLinkedInteractionState
        {
            Policy = _linkGroup.Policy,
            MemberCount = _linkGroup.Members.Count,
            PropagateSelection = _propagateSelection,
            PropagateProbe = _propagateProbe,
            PropagateMeasurement = _propagateMeasurement,
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_propagateSelection || _propagateMeasurement)
        {
            foreach (var member in _linkGroup.Members)
            {
                member.SelectionReported -= OnSelectionReported;
            }
        }

        _disposed = true;
    }

    private void OnSelectionReported(object? sender, SurfaceChartSelectionReportedEventArgs e)
    {
        if (_isPropagating)
        {
            return;
        }

        if (sender is not VideraChartView sourceChart)
        {
            return;
        }

        _isPropagating = true;
        try
        {
            foreach (var member in _linkGroup.Members)
            {
                if (ReferenceEquals(member, sourceChart))
                {
                    continue;
                }

                if (_propagateSelection)
                {
                    member.TryCreateSelectionReport(e.Report.ScreenStart, e.Report.ScreenEnd, out _);
                }

                if (_propagateMeasurement)
                {
                    member.TryCreateSelectionMeasurementReport(e.Report.ScreenStart, e.Report.ScreenEnd, out _);
                }
            }
        }
        finally
        {
            _isPropagating = false;
        }
    }
}
