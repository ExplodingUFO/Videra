# Phase 427: Linked Interaction and Selection Propagation - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning
**Mode:** Autonomous (smart discuss)
**Bead:** `Videra-7tqx.3`

## Phase Boundary

Phase 427 makes linked panel interaction useful and explicit without hiding state
ownership. It adds CameraOnly and AxisOnly link policies, propagates
probe/selection/measurement context across linked panels, and adds support
summaries that describe linked interaction surfaces truthfully.

## User Constraints (from Phase 425)

- Beads are the task, state, and handoff spine.
- Split tasks small and identify dependencies before implementation.
- Use isolated worktrees and branches for parallel beads when write scopes do
  not block each other.
- Every worker must have a responsibility boundary, write scope, validation
  command, and handoff notes.
- Avoid god code.
- Do not add compatibility layers, downshift behavior, fallback behavior, old
  chart controls, or fake validation evidence.
- Keep implementation simple and direct.

## Decisions

### D-01: Link Policy Implementation

Phase 426 created `SurfaceChartLinkGroup` with `FullViewState` policy and
`NotSupportedException` stubs for `CameraOnly` and `AxisOnly`. Phase 427
implements these two policies:

- **CameraOnly**: Synchronizes camera position/rotation only, not data window
  or axis bounds. Uses a subset of `ViewState` fields.
- **AxisOnly**: Synchronizes axis bounds/limits only, not camera position.
  Uses `Plot.Axes` state.

Both policies use the same pairwise link mechanism as `FullViewState` but filter
which `ViewState` fields are synchronized.

### D-02: Selection Propagation Model

Selection propagation is host-owned, not chart-owned. When a probe or selection
event fires on one chart, the host can forward it to linked charts:

- `SurfaceChartSelectionPropagator` listens to `SelectionReported` events on
  registered charts
- When a selection fires on chart A, the propagator calls
  `TryCreateSelectionReport` on linked charts with the same data coordinates
- The propagated selection is a host-owned report, not a chart-owned state
- The propagator does NOT automatically synchronize selections — the host
  must explicitly enable propagation per link group

### D-03: Probe Propagation Model

Similar to selection, probe propagation is host-owned:

- `SurfaceChartProbePropagator` listens to probe events on registered charts
- When a probe fires on chart A, the propagator forwards the probe coordinates
  to linked charts
- Linked charts highlight the corresponding data point if it exists
- Probe propagation is opt-in per link group

### D-04: Measurement Context Propagation

Measurement reports are already host-owned immutable results. Phase 427 adds:

- `SurfaceChartMeasurementPropagator` that can create measurement reports on
  linked charts using the same anchor points
- This enables "compare measurements across charts" workflows
- Measurement propagation is opt-in per link group

### D-05: Linked Interaction Support Summary

Extends the existing workspace evidence to include linked interaction state:

- Which charts are in which link group
- What policy each link group uses
- Which interaction surfaces are active (probe, selection, measurement)
- Whether propagation is enabled for each surface
- Evidence boundaries (what is runtime truth vs what is not)

### D-06: Demo Integration

The demo adds a linked interaction scenario:

- A new `SurfaceDemoScenario.LinkedInteraction` scenario id
- Two charts linked with `FullViewState` policy
- Probe propagation enabled — probing one chart highlights the same point on
  the other
- A toolbar showing link group info and propagation status
- A "Copy linked interaction evidence" button

## Canonical References

- `.planning/phases/426-native-multi-chart-analysis-workspace/426-CONTEXT.md` — workspace decisions
- `.planning/phases/426-native-multi-chart-analysis-workspace/426-RESEARCH.md` — link group research
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartLinkGroup.cs` — existing link group with FullViewState
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.LinkedViews.cs` — pairwise link API
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs` — probe/selection APIs
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartSelectionReport.cs` — selection report type
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartMeasurementReport.cs` — measurement report type

## Existing Code Insights

### Reusable Assets
- `SurfaceChartLinkGroup` — already has pairwise link mechanism and re-entrancy guard
- `VideraChartView.SelectionReported` event — fires when selection occurs
- `TryCreateSelectionReport` — creates host-owned selection report
- `TryResolveProbe` — resolves probe at coordinates
- `TryCreateSelectionMeasurementReport` — creates measurement from selection
- `SurfaceChartWorkspaceEvidence` — existing evidence formatter pattern

### Established Patterns
- Host-owned immutable results (selection, measurement reports)
- Opt-in behavior per link group
- Disposable lifecycle for propagators
- Evidence as bounded text records

### Integration Points
- `SurfaceChartLinkGroup.cs` — where CameraOnly/AxisOnly policies are implemented
- `SurfaceChartWorkspace.cs` — where propagators are registered
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` — where linked interaction scenario wires in

## Deferred Ideas

- Cross-chart axis group facade (applying axis limits across charts) — may belong to Phase 428 streaming
- Deep benchmark-driven propagation performance — Phase 428
- Linked interaction cookbook recipes — Phase 429
- Linked interaction CI/release-readiness gates — Phase 430

---

*Phase: 427-linked-interaction-and-selection-propagation*
*Context gathered: 2026-04-30*
