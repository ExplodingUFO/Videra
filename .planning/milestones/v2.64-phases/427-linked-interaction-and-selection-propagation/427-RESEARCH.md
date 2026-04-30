# Phase 427: Linked Interaction and Selection Propagation - Research

**Researched:** 2026-04-30
**Domain:** .NET/Avalonia linked view-state filtering, host-owned propagation of probe/selection/measurement across linked charts
**Confidence:** HIGH

## Summary

Phase 427 makes linked panel interaction useful and explicit. It has three
concerns: (1) implement CameraOnly and AxisOnly link policies by filtering
which `SurfaceViewState` fields are synchronized, (2) add host-owned
propagators for probe, selection, and measurement context across linked
panels, and (3) extend workspace evidence to describe linked interaction
surfaces truthfully.

The existing `VideraChartViewLink` synchronizes the full `SurfaceViewState`
record by listening to `PropertyChanged` and copying the entire value. For
CameraOnly, the link must copy only the `Camera` field while preserving the
target's `DataWindow`. For AxisOnly, the link must copy only the `DataWindow`
while preserving the target's `Camera`. Both require a new filtered link
implementation because `SurfaceViewState` is a readonly record struct — you
cannot update a single field without constructing a new instance.

Selection propagation is straightforward: `SelectionReported` fires on pointer
release with a `SurfaceChartSelectionReport` containing sample-space
coordinates. The propagator listens on registered charts, and when a selection
fires, calls `TryCreateSelectionReport` on linked charts with the same sample
coordinates. Probe propagation follows the same pattern via `TryResolveProbe`.
Measurement propagation creates measurement reports on linked charts from the
same anchor points.

**Primary recommendation:** Implement filtered link classes
(`VideraChartViewCameraOnlyLink`, `VideraChartViewAxisOnlyLink`) alongside
the existing full link, add `SurfaceChartInteractionPropagator` as a single
host-owned class that handles probe, selection, and measurement propagation
opt-in per link group, and extend `SurfaceChartWorkspaceEvidence` to describe
linked interaction surfaces.

## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** CameraOnly and AxisOnly link policies use the same pairwise link
  mechanism as FullViewState but filter which `ViewState` fields are
  synchronized.
- **D-02:** Selection propagation is host-owned, not chart-owned. Propagator
  listens to `SelectionReported` events and calls `TryCreateSelectionReport`
  on linked charts. Propagation is opt-in per link group.
- **D-03:** Probe propagation is host-owned. Propagator forwards probe
  coordinates to linked charts. Linked charts highlight the corresponding
  data point if it exists. Opt-in per link group.
- **D-04:** Measurement propagation is host-owned. Propagator creates
  measurement reports on linked charts using same anchor points. Opt-in per
  link group.
- **D-05:** Linked interaction support summaries describe which charts are in
  which link group, what policy each group uses, which interaction surfaces
  are active, and evidence boundaries.
- **D-06:** Demo adds `SurfaceDemoScenario.LinkedInteraction` with two linked
  charts, probe propagation enabled, toolbar showing link group info, and a
  "Copy linked interaction evidence" button.

### Claude's Discretion

None explicitly marked — all decisions are locked.

### Deferred Ideas (OUT OF SCOPE)

- Cross-chart axis group facade — may belong to Phase 428 streaming
- Deep benchmark-driven propagation performance — Phase 428
- Linked interaction cookbook recipes — Phase 429
- Linked interaction CI/release-readiness gates — Phase 430

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| LINK-01 | Users can link camera, axis, or view-state behavior across selected panels through bounded native contracts. | CameraOnly/AxisOnly filtered link classes over existing `VideraChartViewLink` pattern. |
| LINK-02 | Users can propagate probe, selection, or measurement context across linked panels while keeping selection ownership explicit in the host. | `SurfaceChartInteractionPropagator` listening to `SelectionReported` events and calling `TryResolveProbe`/`TryCreateSelectionReport`/`TryCreateSelectionMeasurementReport` on linked charts. |
| LINK-03 | Linked interaction support summaries explain which panels are linked, which interaction surfaces are active, and which evidence is runtime truth. | Extended `SurfaceChartWorkspaceEvidence` with linked interaction section. |

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| CameraOnly/AxisOnly link filtering | Library (Workspace/) | — | Library owns link infrastructure; host creates groups with policy |
| Selection propagation | Library (Workspace/) | — | Library provides propagator; host enables per link group |
| Probe propagation | Library (Workspace/) | — | Library provides propagator; host enables per link group |
| Measurement propagation | Library (Workspace/) | — | Library provides propagator; host enables per link group |
| Linked interaction evidence | Library (Workspace/) | — | Library extends evidence formatter |
| Demo linked interaction scenario | Demo (Views/) | — | Demo wires two charts with link group and propagation |

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| SurfaceChartLinkGroup | current | N-chart link group with policy | Already exists with FullViewState; extends to CameraOnly/AxisOnly |
| VideraChartViewLink | current | Pairwise view-state sync | Proven disposable pattern with re-entrancy guard |
| VideraChartView.SelectionReported | current | Selection event source | Fires on pointer release with immutable report |
| VideraChartView.TryResolveProbe | current | Probe resolution | Resolves probe at screen position without changing state |
| VideraChartView.TryCreateSelectionReport | current | Selection report creation | Creates host-owned report at screen coordinates |
| SurfaceChartWorkspace | current | Chart registration and status | Already tracks panels, link groups, evidence |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| SurfaceViewState | current | View-state record | Contains Camera + DataWindow + DisplaySpace |
| SurfaceCameraPose | current | Camera pose record | Target, YawDegrees, PitchDegrees, Distance, FieldOfViewDegrees |
| SurfaceDataWindow | current | Data window record | StartX, StartY, Width, Height |
| SurfaceChartSelectionReport | current | Selection report type | Screen, sample, and axis coordinates |
| SurfaceChartMeasurementReport | current | Measurement report type | Start/end anchors with deltas |
| SurfaceProbeInfo | current | Probe info type | Resolved probe data point |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Separate link classes per policy | Single link class with policy switch | Separate classes are simpler; single class would need complex field-copying logic |
| Single propagator class | Separate propagator per interaction type | Single class is simpler for the host to manage; separate classes add registration overhead |
| Sample-space propagation | Screen-space propagation | Sample-space is coordinate-system independent; screen-space would break when charts have different view sizes |

## Architecture Patterns

### System Architecture Diagram

```
Consumer App / Demo
       |
       v
+---------------------------+
| SurfaceChartWorkspace     |
|   Register(chart, info)   |
|   LinkGroup.Add(chart)    |
|   Propagator.Register()   |
+---------------------------+
       |         |         |
       v         v         v
+----------+ +----------+ +----------+
| Chart A  | | Chart B  | | Chart C  |
+----------+ +----------+ +----------+
       |         |         |
       v         v         v
+-----------------------------------+
| SurfaceChartLinkGroup             |
|   Policy: CameraOnly|AxisOnly     |
|           |FullViewState          |
|   Pairwise filtered links         |
+-----------------------------------+
       |
       v
+-----------------------------------+
| SurfaceChartInteractionPropagator |
|   SelectionReported -> propagate  |
|   Probe move -> propagate         |
|   Measurement -> propagate        |
|   Opt-in per link group           |
+-----------------------------------+
```

### Recommended Project Structure

```
src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/
    SurfaceChartLinkGroup.cs              # [modify] remove NotSupportedException for CameraOnly/AxisOnly
    VideraChartViewCameraOnlyLink.cs      # [new] pairwise camera-only sync
    VideraChartViewAxisOnlyLink.cs        # [new] pairwise axis-only sync
    SurfaceChartInteractionPropagator.cs  # [new] probe/selection/measurement propagation
    SurfaceChartLinkedInteractionState.cs # [new] record describing active interaction surfaces
    SurfaceChartWorkspaceEvidence.cs      # [modify] add linked interaction section

tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/
    SurfaceChartLinkGroupTests.cs         # [modify] CameraOnly/AxisOnly tests
    SurfaceChartInteractionPropagatorTests.cs  # [new] propagation tests

samples/Videra.SurfaceCharts.Demo/
    Services/SurfaceDemoScenario.cs       # [modify] add LinkedInteraction scenario
    Views/MainWindow.axaml.cs             # [modify] wire linked interaction scenario
```

### Pattern 1: Filtered Link Class (CameraOnly)

**What:** `VideraChartViewCameraOnlyLink` listens to `ViewStateProperty` changes on both charts but only copies the `Camera` field from the source to the target, constructing a new `SurfaceViewState` with the target's existing `DataWindow` and `DisplaySpace`.

**When to use:** When the host wants to synchronize camera orbit/zoom across charts without changing which data region each chart shows.

**Example:**
```csharp
// Source: Extension of VideraChartViewLink pattern from LinkedViews.cs
public sealed class VideraChartViewCameraOnlyLink : IDisposable
{
    private readonly VideraChartView _first;
    private readonly VideraChartView _second;
    private bool _disposed;
    private bool _isSynchronizing;

    internal VideraChartViewCameraOnlyLink(VideraChartView first, VideraChartView second)
    {
        _first = first;
        _second = second;
        _first.PropertyChanged += OnFirstPropertyChanged;
        _second.PropertyChanged += OnSecondPropertyChanged;
        CopyCamera(_first, _second);
    }

    private void CopyCamera(VideraChartView source, VideraChartView target)
    {
        if (_disposed || _isSynchronizing) return;
        _isSynchronizing = true;
        try
        {
            var sourceState = source.ViewState;
            var targetState = target.ViewState;
            target.ViewState = new SurfaceViewState(
                targetState.DataWindow,
                sourceState.Camera,
                targetState.DisplaySpace);
        }
        finally
        {
            _isSynchronizing = false;
        }
    }
    // PropertyChanged handlers call CopyCamera instead of CopyViewState
}
```

### Pattern 2: Filtered Link Class (AxisOnly)

**What:** `VideraChartViewAxisOnlyLink` listens to `ViewStateProperty` changes but only copies the `DataWindow` field, preserving the target's existing `Camera` pose.

**When to use:** When the host wants to synchronize which data region is visible (pan/zoom of data window) without changing camera orbit angles.

**Example:**
```csharp
// Same structure as CameraOnly link but copies DataWindow instead of Camera
private void CopyDataWindow(VideraChartView source, VideraChartView target)
{
    if (_disposed || _isSynchronizing) return;
    _isSynchronizing = true;
    try
    {
        var sourceState = source.ViewState;
        var targetState = target.ViewState;
        target.ViewState = new SurfaceViewState(
            sourceState.DataWindow,
            targetState.Camera,
            targetState.DisplaySpace);
    }
    finally
    {
        _isSynchronizing = false;
    }
}
```

### Pattern 3: Interaction Propagation

**What:** `SurfaceChartInteractionPropagator` registers on a `SurfaceChartLinkGroup` and subscribes to `SelectionReported` events on all members. When a selection fires on one chart, the propagator creates equivalent selection reports on linked charts using the same sample-space coordinates. Probe and measurement propagation follow the same pattern.

**When to use:** When the host wants linked charts to show the same selection highlight or probe location.

**Example:**
```csharp
// Source: Extension of existing SelectionReported event pattern
public sealed class SurfaceChartInteractionPropagator : IDisposable
{
    private readonly SurfaceChartLinkGroup _linkGroup;
    private readonly bool _propagateSelection;
    private readonly bool _propagateProbe;
    private readonly bool _propagateMeasurement;
    private bool _disposed;
    private bool _isPropagating;

    public SurfaceChartInteractionPropagator(
        SurfaceChartLinkGroup linkGroup,
        bool propagateSelection = false,
        bool propagateProbe = false,
        bool propagateMeasurement = false)
    {
        _linkGroup = linkGroup;
        _propagateSelection = propagateSelection;
        _propagateProbe = propagateProbe;
        _propagateMeasurement = propagateMeasurement;

        foreach (var member in _linkGroup.Members)
        {
            member.SelectionReported += OnSelectionReported;
        }
    }

    private void OnSelectionReported(object? sender, SurfaceChartSelectionReportedEventArgs e)
    {
        if (_isPropagating || !_propagateSelection) return;
        _isPropagating = true;
        try
        {
            foreach (var member in _linkGroup.Members)
            {
                if (ReferenceEquals(member, sender)) continue;
                // Use sample-space coordinates for coordinate-system independence
                member.TryCreateSelectionReport(
                    e.Report.ScreenStart,
                    e.Report.ScreenEnd,
                    out _); // Host can capture these if needed
            }
        }
        finally
        {
            _isPropagating = false;
        }
    }
}
```

### Pattern 4: Linked Interaction Evidence

**What:** Extended workspace evidence that describes link groups, their policies, active interaction surfaces, and propagation state.

**When to use:** When the host needs diagnostic text describing the linked interaction configuration.

**Example:**
```text
SurfaceCharts workspace evidence
GeneratedUtc: 2026-04-30T12:00:00.000Z
ChartCount: 2
ActiveChartId: chart-a
...
LinkGroupCount: 1
LinkGroup[0]: Policy=FullViewState | Members=chart-a,chart-b | Propagation=Selection,Probe
InteractionSurfaces: Selection=active | Probe=active | Measurement=inactive
EvidenceBoundary: Propagation is runtime truth; linked chart data presence is runtime truth.
```

### Anti-Patterns to Avoid

- **Chart-owned propagation state:** Do not store "is this chart receiving a propagated selection" as chart state. Propagation is host-owned — the propagator manages the re-entrancy guard.
- **Automatic propagation:** Do not automatically propagate when charts are linked. Propagation must be explicitly enabled by the host via the propagator constructor.
- **Screen-space propagation coordinates:** Do not propagate using screen coordinates — different charts have different view sizes. Use sample-space coordinates from the selection report.
- **Modifying VideraChartViewLink:** Do not modify the existing full-view-state link class. Create separate filtered link classes. The existing link remains backward compatible.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Camera-only sync | Custom PropertyChanged filter in link group | `VideraChartViewCameraOnlyLink` class | Separate class is simpler; re-entrancy guard is proven |
| Axis-only sync | Custom PropertyChanged filter in link group | `VideraChartViewAxisOnlyLink` class | Same rationale |
| Probe propagation | Custom event routing from chart A to chart B | `SurfaceChartInteractionPropagator` | Handles re-entrancy, opt-in, disposal |
| Selection propagation | Manual `TryCreateSelectionReport` calls in demo | `SurfaceChartInteractionPropagator` | Library-owned, testable, bounded |
| Linked interaction evidence | String concatenation in demo | Extended `SurfaceChartWorkspaceEvidence` | Keeps evidence format bounded |

**Key insight:** The existing `VideraChartViewLink` pattern is the foundation for all filtered links. The filtered links only differ in which `SurfaceViewState` fields they copy. The propagator is a separate concern that uses the existing `SelectionReported` event and `TryResolveProbe`/`TryCreateSelectionReport` APIs — it does not need new chart-level APIs.

## Common Pitfalls

### Pitfall 1: Filtered Link Re-Entrancy
**What goes wrong:** CameraOnly link copies Camera field, which changes ViewState on target, which fires PropertyChanged, which triggers the link again.
**Why it happens:** Setting `target.ViewState` fires `PropertyChanged` even when only the Camera field changed.
**How to avoid:** Use the same `_isSynchronizing` guard pattern as the existing `VideraChartViewLink`. Set guard before copying, clear after.
**Warning signs:** Stack overflow or UI freeze during linked camera interaction.

### Pitfall 2: Propagation Creates Infinite Loop
**What goes wrong:** Selection on chart A propagates to chart B, which fires SelectionReported on B, which propagates back to A.
**Why it happens:** Propagator subscribes to SelectionReported on all members.
**How to avoid:** Use `_isPropagating` guard. When propagating, suppress re-entry. Same pattern as link re-entrancy.
**Warning signs:** Stack overflow or duplicate selection highlights.

### Pitfall 3: Sample-Space Coordinates Mismatch
**What goes wrong:** Propagated selection shows wrong region on linked chart.
**Why it happens:** Charts may have different data loaded (different metadata, different value ranges). Sample coordinates from chart A may be out of bounds on chart B.
**How to avoid:** `TryCreateSelectionReport` already clamps to metadata bounds. The report will be valid but may highlight a different visual region if the charts have different data extents. Document this as a known limitation.
**Warning signs:** Selection highlight appears at wrong position or is missing on linked chart.

### Pitfall 4: Propagation Does Not Re-Subscribe on Member Change
**What goes wrong:** Adding a chart to a link group after creating the propagator does not get propagation.
**Why it happens:** Propagator subscribes to events at construction time only.
**How to avoid:** Either (a) require propagator creation after all members are added, or (b) expose a method to re-subscribe. Option (a) is simpler and matches the expected usage pattern.
**Warning signs:** New charts in a link group do not receive propagated events.

## Code Examples

### Creating a CameraOnly Link Group

```csharp
// Source: Extension of SurfaceChartLinkGroup pattern
using var linkGroup = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.CameraOnly);
linkGroup.Add(chartA);
linkGroup.Add(chartB);

// chartA and chartB now share camera pose but have independent data windows.
// Orbit/zoom on one chart mirrors on the other; panning data is independent.
```

### Enabling Probe Propagation

```csharp
// Source: Extension of SurfaceChartWorkspace + propagator pattern
var propagator = new SurfaceChartInteractionPropagator(
    linkGroup,
    propagateProbe: true);

// When user hovers over chartA, chartB highlights the same data point (if it exists).
// Propagation is host-owned — disposing the propagator stops it.
```

### Creating Propagated Selection Report

```csharp
// Source: Pattern from VideraChartView.Overlay.cs
// Inside the propagator:
if (sourceChart.TryCreateSelectionReport(
    e.Report.SampleStartX, e.Report.SampleStartY,
    e.Report.SampleEndX, e.Report.SampleEndY,
    out var propagatedSelection))
{
    // The host can use this report for UI updates
}
```

### Linked Interaction Evidence Output

```csharp
// Source: Extension of SurfaceChartWorkspaceEvidence.Create
var evidence = SurfaceChartWorkspaceEvidence.Create(
    workspaceStatus,
    activeRecipeContext,
    linkedInteractionState);
// Evidence now includes:
// - LinkGroup[0]: Policy=CameraOnly | Members=chart-a,chart-b
// - InteractionSurfaces: Probe=active | Selection=inactive | Measurement=inactive
// - EvidenceBoundary: Propagation is runtime truth.
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| FullViewState only, CameraOnly/AxisOnly throw | All three policies implemented | Phase 427 (new) | Enables camera-only or axis-only linking across charts |
| No cross-chart propagation | Host-owned propagator for probe/selection/measurement | Phase 427 (new) | Enables linked interaction workflows |
| Workspace evidence has no linked interaction info | Evidence includes link groups, policies, propagation state | Phase 427 (new) | Diagnostic truth about linked interaction configuration |

**Deprecated/outdated:**
- `NotSupportedException` stubs in `SurfaceChartLinkGroup` for CameraOnly/AxisOnly — replaced by actual implementations.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `SurfaceViewState` can be partially updated by constructing a new instance with different Camera or DataWindow fields | Pattern 1, Pattern 2 | If SurfaceViewState has validation that rejects partial updates, filtered links would fail |
| A2 | Setting `target.ViewState` with a new value that differs only in Camera field still fires `PropertyChanged` on `ViewStateProperty` | Pattern 1 | If PropertyChanged is suppressed for equal values, filtered links may not trigger |
| A3 | `TryCreateSelectionReport` with sample-space coordinates from one chart produces a valid report on a different chart with different data | Pattern 3 | If the method requires screen-space coordinates to match the chart's own view size, propagation would fail |
| A4 | `TryResolveProbe` can accept screen-space coordinates from a different chart's viewport | Pattern 3 | If probe resolution depends on the source chart's projection, cross-chart probe propagation would need coordinate transformation |
| A5 | The demo MainWindow can accommodate a 2-chart linked interaction scenario without conflicting with the existing 4-chart analysis workspace layout | D-06 | May need separate AXAML layout or scenario switching |

## Open Questions

1. **Probe propagation coordinate mapping**
   - What we know: `TryResolveProbe` takes screen-space coordinates. Different charts have different view sizes.
   - What's unclear: Whether probe propagation should use sample-space coordinates (from the probe info) or requires screen-space coordinate transformation.
   - Recommendation: Use sample-space coordinates from `SurfaceProbeInfo` to find the corresponding data point on the linked chart. If the linked chart has the same data, the probe will resolve at the correct screen position for that chart. If the data differs, the probe may not resolve — which is correct behavior.

2. **Link group member subscription lifecycle**
   - What we know: Propagator subscribes to `SelectionReported` on all members at construction time.
   - What's unclear: Whether the propagator should auto-subscribe to charts added after construction.
   - Recommendation: Do not auto-subscribe. Require propagator creation after all members are added. Document this constraint. This matches the simple, explicit pattern used elsewhere.

3. **Measurement propagation anchor resolution**
   - What we know: `TryCreateSelectionMeasurementReport` requires screen-space start/end positions.
   - What's unclear: Whether the propagator can create measurements on linked charts using sample-space anchors from the source chart's measurement.
   - Recommendation: The propagator should use `TryCreateSelectionReport` with sample-space coordinates to create a selection on the linked chart, then call `SurfaceChartMeasurementReport.FromSelection` on that report. This avoids needing screen-space coordinate mapping.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET 8 SDK | Build | Yes | net8.0 target | — |
| Avalonia | UI framework | Yes | 11.3.10 | — |
| xUnit | Testing | Yes | 2.9.3 | — |
| FluentAssertions | Testing | Yes | 8.9.0 | — |
| Avalonia.Headless | Integration tests | Yes | 11.3.10 | — |

**Missing dependencies with no fallback:** None — all required dependencies are available.

## Validation Architecture

> Validation is disabled in config (`workflow.nyquist_validation: false`). Skipping detailed test map.

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 + FluentAssertions 8.9.0 |
| Config file | `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj` |
| Quick run command | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/ --filter "FullyQualifiedName~Workspace"` |
| Full suite command | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/` |

### Wave 0 Gaps

- [ ] `tests/.../Workspace/SurfaceChartLinkGroupTests.cs` — modify: CameraOnly/AxisOnly policy tests (replace NotSupportedException assertions with functional tests)
- [ ] `tests/.../Workspace/SurfaceChartInteractionPropagatorTests.cs` — new: propagation lifecycle, opt-in, re-entrancy
- [ ] `src/.../Workspace/VideraChartViewCameraOnlyLink.cs` — new
- [ ] `src/.../Workspace/VideraChartViewAxisOnlyLink.cs` — new
- [ ] `src/.../Workspace/SurfaceChartInteractionPropagator.cs` — new
- [ ] `src/.../Workspace/SurfaceChartLinkedInteractionState.cs` — new
- [ ] `src/.../Workspace/SurfaceChartWorkspaceEvidence.cs` — modify
- [ ] `src/.../Workspace/SurfaceChartLinkGroup.cs` — modify

## Sources

### Primary (HIGH confidence)
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartLinkGroup.cs` — existing link group with FullViewState and NotSupportedException stubs
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.LinkedViews.cs` — pairwise link pattern with re-entrancy guard
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs` — TryResolveProbe, TryCreateSelectionReport, TryCreateSelectionMeasurementReport
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs` — SelectionReported event, pointer interaction
- `src/Videra.SurfaceCharts.Core/SurfaceViewState.cs` — DataWindow + Camera + DisplaySpace record struct
- `src/Videra.SurfaceCharts.Core/SurfaceCameraPose.cs` — Target, YawDegrees, PitchDegrees, Distance, FieldOfViewDegrees
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartSelectionReport.cs` — selection report with sample-space coordinates
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartMeasurementReport.cs` — measurement report with anchors
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs` — workspace registration and status
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspaceEvidence.cs` — evidence formatter

### Secondary (MEDIUM confidence)
- `.planning/phases/426-native-multi-chart-analysis-workspace/426-RESEARCH.md` — link group research from Phase 426
- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425A-API-WORKSPACE-INVENTORY.md` — API seams and gap analysis

### Tertiary (LOW confidence)
- None — all findings are based on direct codebase inspection.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all components are existing, verified in codebase
- Architecture: HIGH — filtered links directly extend existing `VideraChartViewLink` pattern
- Pitfalls: HIGH — re-entrancy and lifecycle issues are well-understood from existing pairwise link code
- Propagation coordinate mapping: MEDIUM — need to verify sample-space propagation works across charts with different data

**Research date:** 2026-04-30
**Valid until:** 2026-05-14 (stable — depends on existing APIs that are not changing)
