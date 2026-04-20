# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-20)

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** `v1.18 SurfaceCharts Analytics Core` — generalized geometry/axis/scalar contracts, render fast paths, and analytics benchmark evidence

## Current Position

Milestone: `v1.18 SurfaceCharts Analytics Core`
Phase: `96 Scalar Field and Missing-Data Promotion`
Plan: `introduce explicit HeightField / ColorField / mask contracts without breaking the regular-grid source-first path`
Status: `Phase 96 scalar/mask threading is now in code and verified; chart-local render residency preserves ColorField truth, and probe/overlay picking now respects first-class masked holes across both viewport and camera-frame paths`
Last activity: `2026-04-20` — completed Phase 96-03 by threading scalar/mask semantics through render-state residency and probe/overlay consumers, including overlapping detail/overview masked-pick regressions and a GPU recolor regression for independent ColorField truth`

Progress: `[█████□□□] 6/11 requirements complete; Phase 96 source-first adoption compatibility is next`

## Performance Metrics

- Latest fully archived milestone: `v1.16 SurfaceCharts Adoption Surface`
- Operational baseline merged on `master`: `v1.17` repair line
- Active milestone phases: `4`
- Active milestone requirements: `11`

## Accumulated Context

### Decisions

- `v1.7`: `SceneDocument` remains the authoritative viewer scene truth; engine sync and upload now hang off explicit delta/apply/residency services.
- `v1.7`: GPU upload only happens from frame prelude under budget, not from public scene mutation APIs.
- `v1.8`: scene residency dirtying now only moves on scene deltas, resource-epoch changes, or explicit rehydrate events; render cadence itself no longer dirties residency.
- `v1.8`: imported assets now expose shared mesh payloads, `Object3D` retains explicit payload/retention metadata, and upload budgets adapt to runtime mode plus queue pressure.
- `v1.9`: upload telemetry now survives no-op frames, viewer-scene benchmarks exist as a first-class project, and `VideraViewRuntime` delegates scene orchestration through `SceneRuntimeCoordinator`.
- `v1.10`: public alpha consumption now has a narrow minimal sample, a package-based consumer smoke path, benchmark workflow gates, and aligned feedback/support surfaces.
- `v1.11`: the public alpha story is now frozen around one canonical happy path, one diagnostics snapshot contract, and one support/release evidence path.
- `v1.12`: the next user-facing pressure should come from inspection workflows, not from another large architecture or performance cycle.
- `v1.13`: deepen inspection fidelity before widening product scope by improving hit truth, snap semantics, same-API fast paths, and support reproducibility.
- `v1.13`: measurement snap mode lives on `VideraInteractionOptions`, but persists through `VideraInspectionState` so interaction intent and saved inspection truth stay aligned.
- `v1.13`: same-API performance work landed first as cached clip-truth reuse plus preferred live snapshot readback, not as a new GPU-preview API.
- `v1.13`: replayable support artifacts live in `VideraInspectionBundleService` rather than widening `VideraView` into a larger project/session surface.
- `v1.14`: compatibility hardening beat `OpenGL` expansion; the immediate alpha risk was support truth, package-consumer evidence, backend contract drift, and quality-gate ambiguity.
- `v1.14`: packaged consumer quality gates must validate the real local-feed package path, not a raw smoke-project build against stale assumptions.
- `v1.17`: the repair line is now merged on `master`, so benchmark compile drift, SurfaceCharts analyzer debt, and Linux `XWayland` smoke no longer block the next milestone.
- `v1.18`: `SurfaceCharts` should get deeper before it gets wider; generalized data/axis/scalar contracts outrank generic `Chart3D`, public package expansion, or new backend work.

### Pending Todos

- Preserve source-first adoption compatibility while introducing explicit `HeightField` / `ColorField` / mask semantics.
- Add the new analytics benchmarks before scheduling interpolated probe, contour, slice, or extra 3D series work.

### Blockers/Concerns

- The current first-class scalar model is still `value == height == color`, so Phase 96 has to split semantics without breaking the existing tile/cache/overlay callers.
- `Log` axis rendering remains explicitly deferred until raw axis values and display-space coordinates are separated; the current branch now guards against pretending that support exists.
- surface-cache manifest v1 cannot yet represent explicit-grid or non-linear-axis metadata, so cache v2 or richer DTOs remain future work once scalar contracts settle.
- Existing chart benchmarks do not yet measure the interactive hotspots that should drive the next feature wave.
- `.planning/phases/` still contains accumulated historical phase directories; they are being retained as local execution history rather than cleared during this milestone initialization.

## Deferred Items

| Category | Item | Status | Deferred At |
|----------|------|--------|-------------|
| Platform | Native compositor-hosted Wayland embedding | deferred until analytics depth stops outranking platform-scope expansion | `v1.18` |
| Platform | `OpenGL` backend evaluation | deferred unless the analytics roadmap exposes a real backend-coverage blocker | `v1.18` |
| Packages | Publishing `Videra.SurfaceCharts.*` as public consumer packages | deferred until the analytics contracts stabilize | `v1.18` |
| Product shape | Generic `Chart3D` scene abstraction | deferred until at least one more concrete 3D series exists | `v1.18` |
| Analytics | Interpolated probe, contour/wireframe, slicing, and camera presets | deferred until grid/scalar contracts and benchmark evidence land | `v1.18` |
| Series | `WaterfallSeries3D`, `PointLine/ScatterSeries3D`, and `SurfaceMeshSeries3D` | deferred until the core surface analytics contract proves reusable | `v1.18` |

## Session Continuity

Last session: `2026-04-20 +08:00`
Stopped at: `Phase 96-03 complete in code; Phase 96-04 source-first adoption compatibility is next`
Resume file: `.planning/ROADMAP.md`
