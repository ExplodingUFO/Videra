# Linked Interaction

This recipe shows how to synchronize view state and propagate interactions across multiple `VideraChartView` instances using host-owned `SurfaceChartLinkGroup` and `SurfaceChartInteractionPropagator`. Propagation is opt-in: the host creates a propagator with explicit flags for which interaction surfaces to forward.

## Creating a Link Group

A `SurfaceChartLinkGroup` manages pairwise view-state synchronization among its members. Adding a chart creates pairwise links to all existing members. Disposing the group unlinks everything.

```csharp
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;

using var linkGroup = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.FullViewState);
linkGroup.Add(chartA);
linkGroup.Add(chartB);
// Camera, data window, and display space sync across members
```

The `FullViewState` policy synchronizes camera pose, axis limits, and display space. Each pair of charts gets a disposable link managed by the group.

## Filtered Link Policies

Use `CameraOnly` to sync only camera position and rotation. Use `AxisOnly` to sync only the data window and axis bounds.

```csharp
// CameraOnly: syncs camera position/rotation only
using var cameraLink = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.CameraOnly);
cameraLink.Add(chartA);
cameraLink.Add(chartB);

// AxisOnly: syncs data window/axis bounds only
using var axisLink = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.AxisOnly);
axisLink.Add(chartA);
axisLink.Add(chartB);
```

A chart can be in at most one link group at a time. This is enforced by the host, not by the group itself.

## Probe Propagation

Create a `SurfaceChartInteractionPropagator` to forward probe resolution across link group members. The host calls `PropagateProbe` when a probe fires on a source chart. The propagator forwards the probe to all other members.

```csharp
var propagator = new SurfaceChartInteractionPropagator(
    linkGroup,
    propagateProbe: true);

// Host calls when probe fires on a chart
propagator.PropagateProbe(sourceChart, screenPosition);
```

A re-entrancy guard prevents infinite propagation loops when linked charts fire events back through the propagator.

## Selection Propagation

Enable selection propagation to forward `SelectionReported` events across members. The propagator subscribes to the event on all link group members and forwards selections to non-sender members.

```csharp
var propagator = new SurfaceChartInteractionPropagator(
    linkGroup,
    propagateSelection: true);
// SelectionReported events automatically propagate to linked charts
```

Measurement propagation works the same way — set `propagateMeasurement: true` when creating the propagator.

## Evidence

Capture the propagator's interaction state for diagnostics and support reports.

```csharp
var state = propagator.CaptureInteractionState();
// state.Policy, state.MemberCount
// state.PropagateProbe, state.PropagateSelection, state.PropagateMeasurement
```

The state record is an immutable snapshot. It does not reflect later changes to the propagator configuration.

## Cleanup

Dispose the propagator before the link group. The propagator unsubscribes from chart events on disposal. The link group disposes all pairwise links.

```csharp
propagator.Dispose();
linkGroup.Dispose();
```

Charts remain independent `VideraChartView` instances after the link group is disposed. The host owns all state — charts do not know about propagation or link group membership.
