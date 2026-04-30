# Phase 241: Performance Diagnostics Contract - Context

**Gathered:** 2026-04-27
**Status:** Ready for planning
**Mode:** Autonomous execution from v2.27 roadmap

<domain>
## Phase Boundary

Extend viewer and SurfaceCharts diagnostics with performance counters that distinguish backend-neutral scene counts from renderer cost. Keep this phase to diagnostics contract and support evidence only; do not implement instance batching, Performance Lab, or SurfaceCharts columnar data here.

</domain>

<decisions>
## Implementation Decisions

- Do not fake renderer-cost measurements. Fields that the current backend path cannot measure should be nullable or formatted as `Unavailable`.
- Preserve existing `LastFrameObjectCount`, `LastFrameOpaqueObjectCount`, and `LastFrameTransparentObjectCount` source compatibility and explicitly document them as scene counts.
- Treat scene upload bytes as measured by the existing scene residency flush path.
- Treat resident viewer bytes and software SurfaceCharts resident bytes as estimates when computed from retained residency or geometry data.
- Keep SurfaceCharts diagnostics chart-local through `SurfaceChartRenderSnapshot` and `SurfaceChartRenderingStatus`.

</decisions>

<code_context>
## Existing Code Insights

- Viewer public diagnostics flow through `VideraBackendDiagnostics`, `VideraViewSessionBridge.CreateDiagnosticsSnapshot(...)`, and `VideraDiagnosticsSnapshotFormatter`.
- Scene residency already tracks pending/uploaded bytes and approximate per-entry GPU bytes.
- SurfaceCharts rendering status flows from `SurfaceChartRenderSnapshot` to `SurfaceChartRenderingStatus`, then through `SurfaceChartView.RenderingStatus`.
- SurfaceCharts GPU resident tiles already know exact resident resource bytes; software resident tiles can estimate bytes from geometry vertices, scalar samples, and indices.

</code_context>

<specifics>
## Specific Ideas

- Add nullable viewer fields for `LastFrameDrawCallCount`, `LastFrameInstanceCount`, `LastFrameVertexCount`, and `PickableObjectCount`.
- Add viewer fields for `ResidentResourceCount` and `ResidentResourceBytes`.
- Add SurfaceCharts fields for `VisibleTileCount` and `ResidentTileBytes`.
- Update support docs and tests so metric source semantics are visible.

</specifics>

<deferred>
## Deferred Ideas

- Actual draw-call and instance count measurement for instance batches belongs to Phase 243.
- Benchmark thresholds and evidence promotion belong to Phase 244.
- Demo Performance Lab belongs to Phase 245.
- SurfaceCharts columnar scatter diagnostics belong to Phase 246.

</deferred>
