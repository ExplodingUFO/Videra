# Phase 426: Native Multi-Chart Analysis Workspace - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning
**Mode:** Autonomous (smart discuss)
**Bead:** `Videra-7tqx.2`

## Phase Boundary

Phase 426 adds bounded native multi-chart analysis layout affordances for
comparing multiple SurfaceCharts panels. It must NOT create a generic workbench
shell, broad editor, or plugin host. The workspace is a thin coordination layer
over existing `VideraChartView` instances.

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

### D-01: Workspace State Model

The workspace is a host-owned record, not a new Avalonia control. It tracks a
collection of registered `VideraChartView` instances with metadata. The
workspace does NOT own chart lifecycle — charts are created and destroyed by the
host (demo or consumer app), and the workspace only tracks registration.

**Pattern:** `SurfaceChartWorkspace` as a simple record/class that:
- Accepts chart registration with a label and chart-kind tag
- Tracks active chart identity (which chart is "focused")
- Provides enumeration of registered charts
- Is disposable and clears references on dispose

### D-02: Per-Panel Metadata

Each registered chart carries:
- `ChartId` (string or Guid) — stable identity for the panel
- `Label` (string) — user-visible name (e.g., "Surface A", "Contour B")
- `ChartKind` (enum) — surface, waterfall, scatter, bar, contour
- `RecipeContext` (string, optional) — which cookbook recipe or scenario
  produced this chart

This metadata is stored in a `SurfaceChartPanelInfo` record. The workspace
provides a `GetPanelInfo(VideraChartView)` accessor.

### D-03: Link Group Model

Extend the existing pairwise `LinkViewWith` into a group-based model:
- `SurfaceChartLinkGroup` owns a set of linked chart views
- Link policies: `CameraOnly`, `AxisOnly`, `FullViewState` (default)
- Group is disposable; disposing the group unlinks all members
- A chart can be in at most one link group at a time
- The existing `VideraChartViewLink` pairwise API remains for backward compat;
  the new group API is additive

### D-04: Aggregate Status Surface

`SurfaceChartWorkspaceStatus` composes:
- Chart count and per-chart rendering status summary
- Link group count and health
- Active chart identity
- Per-chart dataset scale (series count, point count if available)
- Overall workspace readiness (all charts ready = ready)

This is a snapshot record, not a live observable. Consumers call
`CaptureWorkspaceStatus()` when they need the current state.

### D-05: Workspace Evidence Format

Workspace evidence is a bounded text record (similar to
`SurfaceDemoSupportSummary`) that describes:
- Active panel identity, chart kind, recipe context
- Per-chart rendering status
- Link group membership
- Dataset scale per chart
- Timestamp

This is NOT a workbench export or report generator. It is a support-readable
text block that a consumer can copy for diagnostics.

### D-06: Demo Integration Approach

The demo adds a bounded multi-chart analysis scenario:
- A new `SurfaceDemoScenario.AnalysisWorkspace` scenario id
- A layout with 2-4 `VideraChartView` instances in a grid
- A toolbar showing active chart info and link controls
- A "Copy workspace evidence" button

The demo does NOT become a drag-and-drop layout editor. The layout is fixed
per scenario. Scenario catalog, layout, and support summary remain separated
per WORK-03.

### D-07: File Organization

New library code goes in:
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/` — workspace state,
  panel info, link group, aggregate status, evidence formatter

New test code goes in:
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/` — workspace
  contract tests

Demo changes go in:
- `samples/Videra.SurfaceCharts.Demo/` — scenario, view, services

## Canonical References

- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425A-API-WORKSPACE-INVENTORY.md` — API seams and gap analysis
- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425B-DEMO-COOKBOOK-TEMPLATE-INVENTORY.md` — demo/cookbook/template gaps
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.LinkedViews.cs` — existing pairwise link API
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` — chart control shell
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs` — plot model and evidence hooks
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderingStatus.cs` — per-chart status records
- `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs` — existing support summary pattern

## Existing Code Insights

### Reusable Assets
- `VideraChartView` — the single shipped chart control, already has runtime, overlay, rendering status, and Plot ownership
- `Plot.Add.*` — canonical data-loading path for all chart kinds
- `Plot.Series`, `GetSeries<T>`, `ActiveSeries` — per-chart inventory data
- `Plot.Axes.X/Y/Z` — chart-local axis controls with labels, units, bounds, limits
- `ViewState` — camera and data-window state, already used by `LinkViewWith`
- `VideraChartViewLink` — disposable pairwise link pattern
- `SurfaceChartRenderingStatus` and kind-specific status records — per-chart readiness
- `SurfaceDemoSupportSummary` — text-based support evidence pattern

### Established Patterns
- Host-owned immutable results (probe, selection, measurement reports return data, not chart state)
- Disposable lifecycle for links and resources
- Status snapshot records (not live observables)
- Scenario-addressable demo with bounded vocabulary

### Integration Points
- `VideraChartView.LinkedViews.cs` — where group link API extends the existing pairwise API
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` — where workspace scenario wires in
- `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoScenario.cs` — where new scenario id is added
- `samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs` — where workspace recipe is registered

## Deferred Ideas

- Axis group facade (applying axis limits across multiple charts) — belongs to Phase 427 linked interaction
- Cross-chart probe/selection propagation — belongs to Phase 427
- Streaming workspace evidence — belongs to Phase 428
- Workspace cookbook recipes — belongs to Phase 429
- Workspace CI/release-readiness gates — belongs to Phase 430

---

*Phase: 426-native-multi-chart-analysis-workspace*
*Context gathered: 2026-04-30*
