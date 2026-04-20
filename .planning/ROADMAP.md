# Roadmap

## Active Milestone

- `v1.18 SurfaceCharts Analytics Core`
- Status: `active`
- Focus: upgrade `SurfaceCharts` from a source-first surface control into a professional surface analytics core through generalized geometry/scalar contracts, targeted GPU/residency fast paths, and label-gated benchmark coverage for recolor/orbit/probe/churn/cache lookup-miss/resize-rebind contract hotspots

### Phase 95: Surface Geometry Grid and Axis Scale Contracts

**Goal:** introduce generalized surface geometry and axis-scale contracts so `SurfaceCharts` is no longer locked to index-linear regular height fields.
**Depends on:** `v1.17` repair baseline merged on `master`
**Plans:** 4 planned

Plans:

- [x] 95-01: define `SurfaceGeometryGrid` with `RegularGrid` and `ExplicitGrid` shapes that can represent non-uniform coordinates without flattening them into sample indices
- [x] 95-02: add `SurfaceAxisScale` semantics for `Linear`, `DateTime`, and explicit-coordinate axes while preserving existing label/unit contracts; keep `Log` reserved until display-space separation exists
- [x] 95-03: keep `SurfaceMatrix` as the regular-grid convenience type rather than the only first-class surface contract
- [x] 95-04: prove camera-fit and probe math can consume the new geometry/axis abstractions without regressing the existing regular-grid story

Notes:

- `Log` remains a reserved scale kind for future display-space work; constructor-level rejection now prevents the misleading exponential-world-space behavior from escaping into render/probe paths.
- surface-cache v1 still serializes only regular linear metadata, so writer-side guards now reject explicit-grid and non-linear-axis metadata instead of silently degrading it.

### Phase 96: Scalar Field and Missing-Data Promotion

**Goal:** separate analytic scalar semantics from geometry so height, color, and missing-data truth can evolve independently.
**Depends on:** Phase 95
**Plans:** 4 planned

Plans:

- [x] 96-01: introduce explicit `HeightField` and optional `ColorField` contracts so color is no longer implicitly bound to `z`
- [x] 96-02: model masks, holes, and `NaN` regions as first-class data semantics instead of renderer-only fallbacks
- [x] 96-03: thread the new scalar/mask contracts through chart-local render inputs, probes, and overlays without widening `VideraView`
- [x] 96-04: preserve the current source-first adoption path for regular-grid callers while opening the contract for richer analytics payloads

Notes:

- resident render-state caching now preserves independent `ColorField` scalar truth for GPU recolor/update paths instead of silently collapsing back to height values
- masked probe and camera-pick paths now short-circuit at the highest-detail covered tile, including overlapping detail/overview LOD residency, instead of leaking through holes
- source-first docs and repository guard tests now explicitly preserve `SurfaceMatrix` as the narrow regular-grid onboarding path while documenting `SurfaceScalarField`, independent `ColorField`, and first-class `SurfaceMask` as opt-in analytics payloads under the same chart shell

### Phase 97: Rendering Fast Paths and Residency Tightening

**Goal:** land the highest-value implementation upgrades so the generalized analytics contracts do not come with avoidable recolor, shading, or residency cost.
**Depends on:** Phase 96
**Plans:** 4 planned

Plans:

- [x] 97-01: move palette changes onto a shader/LUT recolor path so resident geometry does not rebuild on every colormap change
- [x] 97-02: replace placeholder normals with local-gradient or central-difference normals, including seam-safe handling across tile boundaries
- [x] 97-03: split data ownership from render residency so resident tiles stop unconditionally copying source values through `ToArray()`
- [x] 97-04: validate the fast-path changes against the existing chart-local GPU-first plus software-fallback rendering contract

Notes:

- GPU recolor now updates a dedicated color-map/LUT uniform payload while resident tiles keep stable vertex buffers plus per-tile scalar uniform payloads, using stable `SurfaceColorMap` / `SurfaceTileScalars` binding slots instead of rewriting full per-vertex color data on palette changes
- native D3D11/Metal/Vulkan pipeline creation now recognizes the same palette/scalar binding contract, and Linux lifecycle coverage exercises the real 4-binding surface draw path instead of only repository/fake-backend evidence
- GPU surface shading now derives normals from local position gradients instead of `UnitY` placeholders, and same-level tile neighbors can refresh boundary normals when residency changes bring new edge context online
- resident render tiles now read scalar truth directly from `SurfaceTile.HeightField` / `ColorField` memory instead of cloning arrays during promotion, while GPU scalar payload uploads consume spans over that shared source-owned memory
- Phase 97 validation now includes full `Videra.SurfaceCharts.Core.Tests`, repository/native contract coverage, targeted Avalonia GPU/software fallback integration coverage, and Linux lifecycle compilation plus platform-gated scalar-binding execution coverage

### Phase 98: Analytics Benchmark Expansion and Milestone Truth

**Goal:** measure the new benchmark hotspots (`recolor` / `orbit` / `probe` / `churn` / `cache lookup-miss` / `resize-rebind`) and lock the milestone story so later probe/contour/slice work starts from evidence rather than guesswork.
**Depends on:** Phase 97
**Plans:** 5 planned

Plans:

- [x] 98-01: add benchmark coverage for recolor and orbit GPU contract-path cost in resident chart interactions
- [x] 98-02: add benchmark coverage for probe latency and tile residency churn under interactive camera motion
- [x] 98-03: add benchmark coverage for cache lookup-miss filtering and handle resize/rebind contract-path cost
- [x] 98-04: record a defensible, label-gated baseline that can later become threshold candidates when trend history is stable
- [x] 98-05: align planning/docs truth around `v1.18` as analytics-core deepening while explicitly deferring generic `Chart3D`, public package expansion, and new backend/OpenGL work

Notes:

- the SurfaceCharts benchmark suite now lives in focused files for selection, render-state, cache, probe, and render-host contract hotspots instead of one monolithic benchmark class
- render-host contract benchmarks explicitly use a benchmark-local fake graphics backend, disable software fallback, and are backed by dedicated smoke tests so recolor/orbit/resize-rebind evidence cannot silently degrade into software-path numbers
- cache lookup-miss evidence is now named for what it actually measures: negative-key short-circuit filtering in `SurfaceCacheReader`, not payload I/O against missing cache files
- the validated local Phase 98 baseline was produced with `pwsh -File ./scripts/Run-Benchmarks.ps1 -Suite SurfaceCharts -Configuration Release`; the latest run emitted clean BenchmarkDotNet logs without exception counters and wrote artifacts under `artifacts/benchmarks/surfacecharts` in the benchmark worktree

## Operational Baseline

- `v1.17` repair work is merged on `master`; the benchmark compile, SurfaceCharts analyzer, and Linux `XWayland` smoke regressions no longer define the active milestone
- the latest fully archived milestone remains `v1.16 SurfaceCharts Adoption Surface`

## Recently Shipped

- `v1.16 SurfaceCharts Adoption Surface` — completed locally `2026-04-20`; phases `87-90`; requirements `9/9`; archive snapshot: `.planning/milestones/v1.16-ROADMAP.md`, `.planning/milestones/v1.16-REQUIREMENTS.md`; audit artifact not yet captured at transition
- `v1.15 Repository Guard and Evidence Calibration` — shipped `2026-04-20`; phases `84-86`; requirements `4/4`; audit `pass`; archive: `.planning/milestones/v1.15-ROADMAP.md`, `.planning/milestones/v1.15-REQUIREMENTS.md`, `.planning/milestones/v1.15-MILESTONE-AUDIT.md`
