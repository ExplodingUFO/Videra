---
phase: 26-gpu-resident-path-slimming
generated: 2026-04-16
status: complete
---

# Phase 26 Research

## Current State

- `SurfaceChartRenderState` currently stores `SurfaceChartResidentTile` objects that duplicate expanded CPU render data as `SamplePositions`, `SampleValues`, and `Colors`.
- `SurfaceChartGpuRenderBackend` still rebuilds a `SurfaceRenderScene` from resident tiles on successful GPU renders even though the scene is never drawn by the GPU path.
- The GPU backend creates a fresh index buffer for every tile in `AddOrReplaceTileResources(...)`, even when tiles share the same `SurfacePatchGeometry` shape.

## Relevant Existing Infrastructure

- `SurfacePatchGeometryBuilder` already caches shared patch topology by sample dimensions, which gives Phase 26 a stable key for shared GPU index buffers.
- `SurfaceChartRenderHost` already routes fallback behavior correctly; slimming work can stay below that seam.
- Tests already cover incremental dirty-state truth, GPU fallback truth, and render-host integration.

## Chosen Direction

1. Replace duplicated resident CPU arrays with a software-oriented resident contract that stores one `SurfaceRenderTile` plus the source tile/sample values.
2. Introduce a GPU-specific resident resource contract and topology cache so the GPU backend owns lighter runtime resources without depending on `SoftwareScene`.
3. Keep color mapping CPU-side for now; Phase 27 will move it into backend/shader space.

## Verification Strategy

- Add failing tests proving:
  - successful GPU rendering does not publish `SoftwareScene`
  - same-topology tiles reuse a shared index buffer
  - software resident truth still survives projection/color changes without rebuilding everything incorrectly
- Re-run core rendering tests, GPU fallback tests, Avalonia render-host integration tests, and the benchmark project build.
