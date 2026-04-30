# Phase 17: Large-Dataset Residency, Cache Evolution, and Optional Rust Spike - Context

**Gathered:** 2026-04-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Upgrade the surface-chart data path so large datasets stay interactive on top of the Phase 16 render-host seam. This phase covers view-aware tile scheduling, cancellation and bounded concurrency, cache-read evolution, richer pyramid reducers and tile statistics, and profiling evidence for an optional coarse Rust hotspot seam. It does not change the chart boundary, move orchestration into `VideraView`, or introduce a broad native rewrite.

</domain>

<decisions>
## Implementation Decisions

### Scheduler and residency policy
- **D-01:** Keep the Phase 16 render-host and resident-render-state seam as the downstream truth. Phase 17 changes should live in `SurfaceChartController`, `SurfaceTileScheduler`, cache adapters, and processing code rather than redesigning `SurfaceChartRenderHost` or pushing chart semantics into `VideraView`.
- **D-02:** Replace the current overview-then-sequential-detail request flow with a view-aware scheduler that still guarantees overview availability but prioritizes detail work by the active viewport, selected LOD, and nearby residency value instead of simple row-major order.
- **D-03:** Use generation-based supersession plus cancellation as a hard contract. Viewport or size changes should cancel or ignore stale work so outdated responses never repopulate the active residency set.
- **D-04:** Concurrency must be explicitly bounded. Phase 17 should prefer a bounded neighborhood around the active viewport over the current "prune almost everything and start over" behavior, but it should not introduce unbounded residency growth.

### Cache I/O evolution
- **D-05:** Preserve the current manifest-plus-sidecar cache format as the starting point. Do not begin with a broad cache-format rewrite unless the implementation proves that a small additive metadata change is necessary.
- **D-06:** Evolve `SurfaceCacheReader` toward persistent payload access and additive batch-read APIs so repeated interaction stops reopening, reallocating, and recopying payload data for every tile request.
- **D-07:** Keep `ISurfaceTileSource` compatibility for runtime callers. Any batch or persistent-reader capability should be introduced through additive interfaces or adapters rather than a breaking replacement of the current single-tile contract.

### Pyramid reducers and truthful statistics
- **D-08:** Keep average reduction as the default compatibility behavior, but make pyramid and cache processing extensible enough to support richer reducers and per-tile statistics.
- **D-09:** First-class tile statistics should cover at least min, max, and range or peak-preserving summaries so legends, probes, and future diagnostics are not forced to infer truth from averages alone.
- **D-10:** Distinguish exact values from aggregated values. When overlays or probes fall back to reduced data, the system must preserve truthful semantics instead of presenting average-derived values as exact sample truth.

### Profiling budgets and optional Rust seam
- **D-11:** Add profiling and budget evidence before any native work. Phase 17 should measure scheduler, cache, and processing hotspots in C# first instead of assuming Rust is needed.
- **D-12:** If a Rust seam is justified, keep it coarse-grained and limited to lower-level data paths such as reduction, cache codec, or batched payload processing. UI, scheduler orchestration, and render-host ownership stay in C#.
- **D-13:** Any Rust adoption must keep a C# reference or fallback implementation and parity tests so the native seam remains optional rather than becoming the only supported path.

### Verification and delivery truth
- **D-14:** Verification should rely on deterministic unit and integration tests plus fixture-backed measurements. Do not make a checked-in giant dataset or demo-only responsiveness claims the primary proof for this phase.
- **D-15:** Phase 17 may tighten docs and tests where scheduler, cache, or statistics truth changes, but it should not expand the public chart story beyond what the upgraded data path can actually prove.

### the agent's Discretion
- Exact request-priority scoring formula, queue shapes, and fairness heuristics inside the bounded scheduler.
- The additive API names used for batch reads or persistent payload access, as long as `ISurfaceTileSource` compatibility remains intact.
- Whether a small cache-manifest metadata extension is required, provided it stays additive and justified by tests.
- The profiling harness shape and whether an optional Rust spike becomes a concrete plan item or a no-op decision backed by measurements.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase boundary and milestone constraints
- `.planning/ROADMAP.md` — Phase 17 goal, plan split, dependencies, and success criteria.
- `.planning/REQUIREMENTS.md` — `REND-03`, `DATA-01`, `DATA-02`, `DATA-03`, and `DATA-04`.
- `.planning/PROJECT.md` — current milestone risks, C#-first chart architecture, and the coarse-grained Rust boundary.
- `.planning/STATE.md` — Phase 16 completion state and the current recommended next step.

### Prior phase decisions
- `.planning/phases/16-rendering-host-seam-and-gpu-main-path/16-CONTEXT.md` — render-host seam, GPU-first main path, explicit fallback truth, and the deliberate deferral of scheduler and cache evolution to Phase 17.
- `.planning/phases/16-rendering-host-seam-and-gpu-main-path/16-VERIFICATION.md` — what Phase 16 already proved so Phase 17 does not relitigate renderer ownership.

### Codebase maps
- `.planning/codebase/CONVENTIONS.md` — module boundary and layering rules for `Core`, `Rendering`, `Processing`, and `Avalonia`.
- `.planning/codebase/STRUCTURE.md` — current placement of surface-chart runtime, rendering, and processing responsibilities.
- `.planning/codebase/STACK.md` — .NET, Avalonia, Silk.NET, and optional native interop constraints.
- `.planning/codebase/TESTING.md` — expected unit and integration test layout for this module family.
- `.planning/codebase/CONCERNS.md` — known risks around scheduler behavior, cache reads, and platform truth.

### Scheduler and runtime seams
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartController.cs` — current request supersession flow, detail-prune behavior, and pipeline entry points.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileScheduler.cs` — current overview-first plus sequential detail scheduling seam.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileCache.cs` — requested versus loaded tile tracking and current pruning semantics.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.cs` — control shell composition around controller, cache, and render host.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs` — loaded-tile handoff into `SurfaceChartRenderHost`.

### Core and processing contracts
- `src/Videra.SurfaceCharts.Core/ISurfaceTileSource.cs` — stable single-tile runtime contract that Phase 17 must preserve or adapt around.
- `src/Videra.SurfaceCharts.Core/SurfaceLodPolicy.cs` — current viewport-to-LOD selection and neighborhood expansion.
- `src/Videra.SurfaceCharts.Core/SurfaceLodSelection.cs` — current row-major tile enumeration behavior.
- `src/Videra.SurfaceCharts.Core/SurfacePyramidBuilder.cs` — current average-only reduction and scratch tracking.
- `src/Videra.SurfaceCharts.Core/SurfaceTile.cs` — tile payload and per-tile value-range contract.
- `src/Videra.SurfaceCharts.Core/SurfaceMetadata.cs` — dataset-level value-range and dimensions contract.
- `src/Videra.SurfaceCharts.Processing/SurfaceCacheReader.cs` — eager manifest validation plus per-tile payload reopen and copy behavior.
- `src/Videra.SurfaceCharts.Processing/SurfaceCacheTileSource.cs` — adapter from cache reader to `ISurfaceTileSource`.
- `src/Videra.SurfaceCharts.Processing/SurfaceCacheWriter.cs` — current manifest and payload sidecar layout.
- `src/Videra.SurfaceCharts.Processing/README.md` — current processing-module scope and cache format description.

### Render and truth consumers
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` — renderer ownership seam that Phase 17 should feed instead of redesigning.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderState.cs` — resident-tile and dirty-bucket behavior that benefits from better residency stability.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs` — probe resolution against currently loaded tiles, which motivates truthful aggregated statistics.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs` — current legend range presentation behavior.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs` — overlay refresh path driven by loaded tiles and metadata.

### Existing verification surface
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs` — current overview-first, prune, and retry behavior.
- `tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCacheTileSourceTests.cs` — cache validation and lazy tile loading behavior.
- `tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCacheRoundTripTests.cs` — manifest and payload round-trip plus replacement safety.
- `tests/Videra.SurfaceCharts.Core.Tests/SurfacePyramidBuilderTests.cs` — average reduction and scratch peak expectations.
- `tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartRenderStateTests.cs` — resident render-state contract coverage to keep stable while data-path behavior evolves.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `SurfaceLodPolicy` and `SurfaceLodSelection` already compute a bounded neighborhood around the visible viewport; Phase 17 can evolve prioritization on top of that selection instead of inventing a separate visibility model.
- `SurfaceTileCache` already tracks requested and loaded tiles by generation; it is the natural residency bookkeeping seam for stale-work suppression and bounded retention.
- `SurfaceCacheReader` and `SurfaceCacheTileSource` already separate eager manifest indexing from lazy payload access, which gives Phase 17 a clear place to add persistent or batched reads without disturbing callers.
- `SurfacePyramidBuilder` already owns reduction logic and scratch metrics, so richer reducers and statistics belong in `Core` or `Processing`, not in the Avalonia control layer.
- `SurfaceChartRenderHost` and `SurfaceChartRenderState` already accept incremental loaded-tile updates and maintain resident render state across changes.
- `SurfaceProbeService` and legend overlay presenters are existing consumers of data truth and will benefit from better tile statistics without changing the chart boundary.

### Established Patterns
- Surface-chart responsibilities stay split across `Core`, `Rendering`, `Processing`, and `Avalonia`; backend-neutral logic should not leak into the control shell.
- Runtime contracts are C#-first and additive. New capability should extend or adapt the current contracts rather than replace them wholesale.
- GPU-first rendering with explicit software fallback is already the project truth; Phase 17 improves data delivery into that seam rather than changing backend policy.
- Tests are the primary proof surface: algorithmic behavior lives in unit tests, while control and render orchestration live in headless Avalonia integration tests.

### Integration Points
- `SurfaceChartController.UpdateSource`, `UpdateViewport`, and `UpdateViewSize` are the current points where request pipelines are superseded and stale detail tiles are pruned.
- `SurfaceTileScheduler.RequestOverviewAsync` and `RequestViewportAsync` are the seams for introducing prioritization, cancellation, and bounded concurrency.
- `SurfaceChartView.SyncRenderHost` is where loaded or resident tiles become renderer inputs.
- `SurfaceCacheReader.LoadTileAsync` and `SurfaceCacheTileSource.GetTileAsync` are the current cache-read seam for persistent handles or batched payload access.
- `SurfacePyramidBuilder.Build` is the right layer for reducer and statistics extensibility plus profiling hooks.
- Existing scheduling, cache, and pyramid tests already cover the main contracts that Phase 17 will need to extend.

</code_context>

<specifics>
## Specific Ideas

- `[auto]` Selected all gray areas: scheduler and residency, cache and batch I/O, pyramid reducers and statistics, profiling and Rust seam, verification budgets.
- `[auto]` Chose the recommended default in every area so planning can proceed without another interactive round.
- `[auto]` No phase-matching todos were found, so nothing was folded into scope.
- No extra product references or UX-specific requests were added during discuss-phase; roadmap, requirements, and Phase 16 seam artifacts remain the source of truth.

</specifics>

<deferred>
## Deferred Ideas

- Compositor-native Wayland embedding or broader Linux hosting claims.
- Moving surface charts into `VideraView` or a shared render-session abstraction.
- A broad Rust rewrite of runtime, UI, scheduler, or renderer orchestration.
- New chart interaction or presentation capabilities beyond the existing Phase 14 and Phase 15 contracts.
- A broad cache-format rewrite unless additive metadata changes are proven necessary by implementation and tests.
- Giant checked-in benchmark datasets or demo polish as the primary proof surface; repository-truth and demo packaging belong to Phase 18.

</deferred>

---

*Phase: 17-large-dataset-residency-cache-evolution-and-optional-rust-spike*
*Context gathered: 2026-04-14*
