# Phase 26: GPU Resident Path Slimming - Context

**Gathered:** 2026-04-16
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase tightens the GPU render path so it no longer carries unnecessary software-scene shadow state or duplicate topology resources. The goal is to reduce GPU-path CPU memory and upload churn without widening the public chart/viewer API boundary.

</domain>

<decisions>
## Implementation Decisions

### GPU Resident Contract
- Keep software-fallback residency and GPU residency as separate contracts.
- Do not push any new chart-specific abstractions into `VideraView`; keep the work inside `Videra.SurfaceCharts.Rendering`.
- Treat GPU resident resources as backend-owned objects instead of reusing software-scene structures as the GPU truth.

### Topology and Upload Strategy
- Share index/topology buffers across tiles with identical patch shapes.
- Keep per-tile vertex uploads for this phase; shader-side color mapping stays deferred to Phase 27.
- Remove GPU-path `SoftwareScene` construction rather than trying to optimize it.

### Safety Constraints
- Preserve software fallback behavior and host-visible rendering status truth.
- Preserve overview-first / incremental change-set semantics already shipped in Phases 16 and 25.
- Prefer focused tests around resource reuse and scene publication rather than speculative refactors.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `SurfacePatchGeometryBuilder` already caches topology by `(sampleWidth, sampleHeight)`.
- `SurfaceChartRenderState` already centralizes full-reset / residency / color / projection dirty tracking.
- `SurfaceChartGpuRenderBackend` already owns native backend lifecycle and per-tile GPU resources.

### Established Patterns
- Software and GPU backends share `SurfaceChartRenderChangeSet` truth through `SurfaceChartRenderHost`.
- Rendering tests use fake graphics/resource factories to assert backend behavior without native dependencies.
- Integration tests already compare GPU and software dirty-delta behavior through `SurfaceChartIncrementalRenderingTests`.

### Integration Points
- `SurfaceChartResidentTile` and `SurfaceChartRenderState` define what long-lived resident CPU state exists.
- `SurfaceChartGpuRenderBackend` is where topology sharing and software-scene bypass must land.
- `SurfaceChartRenderHost.SoftwareScene` remains the user-visible seam for software drawing and GPU fallback.

</code_context>

<specifics>
## Specific Ideas

- The GPU backend should stop publishing a `SoftwareScene` during successful native rendering.
- Shared patch topology should remove duplicate index-buffer creation for same-shape tiles.
- Resident software state should avoid duplicating expanded vertex/color arrays when one software render tile already exists.

</specifics>

<deferred>
## Deferred Ideas

- Shader/backend color-map LUT work remains Phase 27.
- Normal generation improvements remain optional until the shader/backend migration decides where normals live.
- Broader processing/native optimization remains outside the committed scope for this phase.

</deferred>
