---
phase: 16-rendering-host-seam-and-gpu-main-path
verified: 2026-04-14T11:22:14.0795179Z
status: passed
score: 3/3 must-haves verified
---

# Phase 16: Rendering Host Seam and GPU Main Path Verification Report

**Phase Goal:** 在不碰 `VideraView` 主线的前提下，引入 chart-specific rendering package、render-host seam、GPU 主路径与 software fallback。  
**Verified:** 2026-04-14T11:22:14.0795179Z  
**Status:** passed  
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
| --- | --- | --- | --- |
| 1 | Surface charts have an explicit renderer seam that does not require new chart semantics on `VideraView`. | ✓ VERIFIED | `Videra.slnx` wires `src/Videra.SurfaceCharts.Rendering/Videra.SurfaceCharts.Rendering.csproj`; `SurfaceChartView` owns `_renderHost` and pushes live inputs through `_renderHost.UpdateInputs(...)`; direct grep found no `VideraView`, `RenderSession`, `VideraViewSessionBridge`, or `IVideraNativeHost` references inside `src/Videra.SurfaceCharts.Rendering` or `src/Videra.SurfaceCharts.Avalonia/Controls`. |
| 2 | The main path renders through a GPU backend while retaining a truthful software fallback. | ✓ VERIFIED | `SurfaceChartRenderHost.Render(...)` prefers GPU on bound native handles and catches backend failures into software with `IsFallback` and `FallbackReason`; `SurfaceChartGpuRenderBackend` allocates GPU buffers/pipeline and frames through `IGraphicsBackend`; `SurfaceChartView` projects host-visible `RenderingStatus` and `RenderStatusChanged`; targeted core + Avalonia fallback tests passed. |
| 3 | The renderer no longer depends on full-scene rebuilds for every viewport/input change. | ✓ VERIFIED | `SurfaceChartRenderState.Update(...)` computes `FullResetRequired`, `ResidencyDirty`, `ColorDirty`, and `ProjectionDirty`; projection-only updates stay incremental; `SurfaceChartSoftwareRenderBackend` rebuilds only on full reset, residency, or color changes; targeted incremental-rendering tests passed for both software and GPU dirty-signature parity. |

**Score:** 3/3 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
| --- | --- | --- | --- |
| `src/Videra.SurfaceCharts.Rendering/Videra.SurfaceCharts.Rendering.csproj` | Chart-local rendering package | ✓ VERIFIED | Exists, targets `net8.0`, references `Videra.Core` and `Videra.SurfaceCharts.Core`, and is added to `Videra.slnx`. |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` | Renderer seam and backend orchestration root | ✓ VERIFIED | Exists, owns `SurfaceChartRenderState`, selects GPU/software backend, and publishes typed snapshot/status. |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs` | GPU-first backend implementation | ✓ VERIFIED | Exists, uses `IGraphicsBackend`, `IResourceFactory`, and `ICommandExecutor` directly, maintains GPU tile resources, and presents through a native handle. |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderState.cs` | Resident renderer state and dirty-boundary truth | ✓ VERIFIED | Exists, keeps resident tiles keyed by `SurfaceTileKey`, builds resident geometry via `SurfaceRenderer.BuildTile(...)`, and emits explicit change sets. |
| `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs` | Control shell wiring to render host | ✓ VERIFIED | Exists, syncs source/tiles/color/viewport/projection/size/native-handle into the host and only draws the software path through `SurfaceScenePainter.DrawScene(...)`. |
| `src/Videra.SurfaceCharts.Avalonia/Controls/ISurfaceChartNativeHost.cs` | Chart-local native host contract | ✓ VERIFIED | Exists alongside chart-local Windows/Linux/macOS host implementations and factory wiring. |
| `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs` | Repository guard for boundary and doc truth | ✓ VERIFIED | Exists, blocks `VideraView` drift and locks README/demo wording for GPU-first, fallback, and Linux host limits. |

### Key Link Verification

| From | To | Via | Status | Details |
| --- | --- | --- | --- | --- |
| `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs` | `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` | `_renderHost.UpdateInputs(...)` | ✓ WIRED | The control pushes metadata, loaded tiles, color map, viewport, projection settings, view size, native handle, and render scale into the host. |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` | `src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs` | GPU-first selection with software fallback catch | ✓ WIRED | Bound native handles route to `_gpuBackend.Render(...)`; exceptions are converted into a software snapshot with `IsFallback = true` and `FallbackReason = ex.Message`. |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` | `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderState.cs` | `_renderState.Update(inputs)` | ✓ WIRED | Render-host ownership now flows through a resident-state/change-set seam before either backend renders. |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderState.cs` | `src/Videra.SurfaceCharts.Core/Rendering/SurfaceRenderer.cs` | `CreateResidentTile(...)` -> `_renderer.BuildTile(...)` | ✓ WIRED | The resident-state seam preserves the existing sample-to-axis/value mapping truth from `SurfaceRenderer`. |
| `src/Videra.SurfaceCharts.Avalonia/README.md` / `samples/Videra.SurfaceCharts.Demo/README.md` | `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs` | Exact sentence guards | ✓ WIRED | Repository tests assert the chart-local renderer seam, GPU-first/software-fallback wording, sibling boundary, and XWayland-only Linux host truth. |

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
| --- | --- | --- | --- | --- |
| `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs` | `LoadedTiles`, `Metadata`, `ColorMap`, `Viewport`, `NativeHandle` | `Source`, `_tileCache.GetLoadedTiles()`, `Viewport`, `_cameraController.ProjectionSettings`, `_nativeHost?.CurrentHandle` | Yes | ✓ FLOWING |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` | `LastChangeSet`, `Snapshot`, `RenderingStatus` | `_renderState.Update(inputs)` + chosen backend render result | Yes | ✓ FLOWING |
| `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderState.cs` | `ResidentTiles`, color deltas, projection dirtiness | `SurfaceRenderer.BuildTile(...)`, incoming `LoadedTiles`, incoming `ColorMap`, viewport/projection/view-size changes | Yes | ✓ FLOWING |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
| --- | --- | --- | --- |
| Core render seam, GPU fallback, and incremental state tests | `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderHostTests|FullyQualifiedName~SurfaceChartGpuFallbackTests|FullyQualifiedName~SurfaceChartRenderStateTests|FullyQualifiedName~SurfaceRendererInputTests"` | Passed: 15/15 | ✓ PASS |
| Avalonia render-host, GPU/fallback, and incremental integration tests | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderHostIntegrationTests|FullyQualifiedName~SurfaceChartViewGpuFallbackTests|FullyQualifiedName~SurfaceChartIncrementalRenderingTests|FullyQualifiedName~SurfaceScenePainterTests"` | Passed: 15/15 | ✓ PASS |
| Repository boundary/doc guards | `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests"` | Passed: 5/5 | ✓ PASS |
| Chart renderer stays off `VideraView` mainline | `rg -n "SurfaceChartRenderHost|SurfaceChartGpuRenderBackend|Videra\\.SurfaceCharts\\.Rendering" src/Videra.Avalonia/Controls/VideraView.cs src/Videra.Avalonia/Controls/VideraView.Overlay.cs` | `no matches` | ✓ PASS |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
| --- | --- | --- | --- | --- |
| `REND-01` | `16-01`, `16-02` | Surface charts render through a GPU-first renderer that keeps a software fallback path for unsupported or test environments. | ✓ SATISFIED | `SurfaceChartRenderHost` prefers GPU on bound handles, `SurfaceChartGpuRenderBackend` uses graphics abstractions directly, fallback propagates `IsFallback` and `FallbackReason`, `SurfaceChartView` exposes `RenderingStatus`, and the core/Avalonia GPU fallback suites passed. `REQUIREMENTS.md` still marks this item pending, but the code/tests/docs/guards in this checkout satisfy it. |
| `REND-02` | `16-01`, `16-03` | The main renderer keeps incremental scene and residency state instead of rebuilding the full scene snapshot on every viewport/input change. | ✓ SATISFIED | `SurfaceChartRenderState` tracks resident tiles plus explicit dirty buckets; `SurfaceChartSoftwareRenderBackend` skips full scene rebuilds on projection-only changes; integration tests prove repeated viewport changes reuse resident geometry and that GPU/software share the same dirty truth. |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
| --- | --- | --- | --- | --- |
| `.planning/REQUIREMENTS.md` | 30, 82 | `REND-01` still marked pending after implementation landed | ℹ️ Info | Planning metadata lags behind the verified code/tests/docs state, but this does not block the Phase 16 goal. |

### Human Verification Required

None. For this phase, the renderer seam, fallback truth, incremental state behavior, doc truth, and repository boundary were all verified with direct code inspection plus fresh targeted test runs.

### Gaps Summary

None blocking goal achievement. Phase 16 delivers the chart-local rendering package, render-host seam, GPU-first path with explicit software fallback, and incremental renderer-state behavior without pulling chart semantics into `VideraView`.

---

_Verified: 2026-04-14T11:22:14.0795179Z_  
_Verifier: Claude (gsd-verifier)_
