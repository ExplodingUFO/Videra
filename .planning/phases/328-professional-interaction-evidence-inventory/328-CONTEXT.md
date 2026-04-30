# Phase 328: Professional Interaction Evidence Inventory - Context

**Gathered:** 2026-04-28  
**Status:** Ready for planning  
**Bead:** Videra-7l7

<domain>
## Phase Boundary

v2.46 starts from v2.45's professional output layer. The next useful slice is not renderer/backend expansion; it is a supportable interaction evidence chain that lets a user attach selection, probe, scene, chart, diagnostics, and environment truth to a report.

Current source truth:

- Viewer inspection state is already typed through `VideraInspectionState`, `SelectionState`, `Annotations`, `Measurements`, `ClippingPlanes`, and `VideraInspectionBundleService`.
- Viewer diagnostics snapshots already exist through `VideraDiagnosticsSnapshotFormatter`.
- `VideraInteractionDiagnostics` exposes capability flags but not a formatted interaction evidence report.
- `InteractionSample` demonstrates host-owned selection/annotation/measurement workflows and bundle export/import.
- `AvaloniaWorkbenchSample` has scene evidence, chart output evidence, diagnostics snapshots, and a support capture formatter.
- SurfaceCharts has public `SurfaceProbeInfo` / `SurfaceProbeResult`, chart-local `OverlayOptions`, pinned/hovered probe state internally, and existing probe overlay tests.
- `SurfaceChartOutputEvidence` captures palette/precision/output metadata, but probe evidence is currently formatted inside Avalonia overlay internals rather than as a reusable chart-local evidence contract.
</domain>

<decisions>
## Implementation Decisions

1. Phase 329 should add a small Avalonia-side viewer interaction evidence formatter rather than changing `Core` scene truth.
2. Phase 330 should add a chart-local SurfaceCharts probe evidence formatter rather than exposing internal overlay state or moving chart semantics into `VideraView`.
3. Phase 331 should integrate both evidence contracts into the optional workbench support capture only after 329 and 330 land.
4. Phase 332 should update docs, roadmap, and guardrails after the implementation phases are complete.
</decisions>

<non_goals>
## Non-Goals

- No backend expansion.
- No animation, shadows, post-processing, material graphs, WebGL/OpenGL, or second UI adapter.
- No ECS, runtime scripting, runtime gizmo/manipulator framework, or scene editor.
- No broad chart-family expansion.
- No compatibility layer, hidden fallback/downshift path, or god-code aggregation surface.
- No default dependency from `Videra.Core` or base `Videra.Avalonia` onto optional workbench behavior.
</non_goals>
