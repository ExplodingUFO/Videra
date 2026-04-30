---
phase: 409
bead: Videra-1f8
status: inventory
scope: "Phase 409 native API/performance surface inventory only"
allowed_write_set:
  - ".planning/phases/409-native-cookbook-and-ci-inventory/409B-NATIVE-PERFORMANCE-INVENTORY.md"
---

# Phase 409B Native Performance Inventory

This file is evidence-only. It inventories the current Videra-native chart
ownership/API surfaces and performance-sensitive data paths so Phase 411 can
pick minimal implementation slices without product/demo/test edits during Phase
409.

## Native Ownership and API Surfaces

- `VideraChartView` is the shipped Avalonia control owner for SurfaceCharts.
  It owns public `RenderingStatus`, chart-family status records, and the
  chart-local `Plot3D` instance (`src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs:53`,
  `:58`, `:63`, `:68`, `:73`, `:94`). This confirms the API surface is native
  Videra control + plot ownership, not old chart-view wrappers.
- `VideraChartView` exposes host-visible camera/data-window operations through
  `FitToData()` and `ResetCamera()` (`VideraChartView.Core.cs:166`, `:175`) and
  host-owned view state through the `ViewState` styled property
  (`VideraChartView.Properties.cs:16`, `:30`). Linked views copy that
  host-owned view state between controls (`VideraChartView.LinkedViews.cs:58`,
  `:72`).
- The concise native add API is `VideraChartView.Plot.Add.*`, backed by
  `Plot3DAddApi`: `Surface(ISurfaceTileSource|SurfaceMatrix|double[,])`,
  `Waterfall(ISurfaceTileSource|SurfaceMatrix|double[,])`,
  `Scatter(ScatterChartData|double[] x/y/z)`, `Bar(double[]|BarChartData)`, and
  `Contour(double[,]|SurfaceScalarField|ContourChartData)`
  (`Plot3DAddApi.cs:21`, `:30`, `:39`, `:47`, `:56`, `:65`, `:73`, `:82`,
  `:92`, `:104`, `:115`, `:155`, `:166`).
- Add APIs return typed Videra handles, not ScottPlot-compatible plottables:
  `SurfacePlot3DSeries`, `WaterfallPlot3DSeries`, `ScatterPlot3DSeries`,
  `BarPlot3DSeries`, and `ContourPlot3DSeries` are sealed native handle types
  (`SurfacePlot3DSeries.cs:8`, `WaterfallPlot3DSeries.cs:8`,
  `ScatterPlot3DSeries.cs:8`, `BarPlot3DSeries.cs:8`,
  `ContourPlot3DSeries.cs:8`). The shared handle base exposes native kind/name
  identity (`Plot3DSeries.cs:33`, `:38`).
- Axes are chart-local native objects. `PlotAxes3D` exposes `X`, `Y`, and `Z`
  axes (`PlotAxes3D.cs:12`, `:20`, `:25`, `:30`), labels/units are mapped onto
  `SurfaceChartOverlayOptions` (`PlotAxes3D.cs:61`, `:75`, `:221-226`), and
  bounds/autoscale/lock behavior is owned by `PlotAxis3D` (`PlotAxes3D.cs:89`,
  `:94`, `:101`, `:127`, `:167`).
- Interactions are native built-ins with host-owned state, not compatibility
  gestures. Runtime/controller paths update `SurfaceViewState` and interaction
  quality (`SurfaceChartRuntime.cs:55`, `:65`, `:100`, `:141`, `:156`,
  `:186`); pointer/keyboard command handling delegates to reset/fit commands
  (`VideraChartView.Input.cs:53`, `:231-232`, `:324`, `:335`).
- Selection/probe state remains explicit and host-visible. Pinned probes are
  toggled through the overlay coordinator (`SurfaceChartOverlayCoordinator.cs:70`)
  and rendered from `SurfaceProbeOverlayState.PinnedProbes`
  (`SurfaceProbeOverlayState.cs:44`). Draggable marker/range recipes are
  explicit native helpers (`SurfaceChartDraggableOverlayRecipes.cs:8`, `:17`,
  `:27`, `:32`, `:51`, `:63`, `:81`), and `VideraChartView` exposes creation
  helpers without owning app state (`VideraChartView.Overlay.cs:142`, `:161`).
- PNG snapshots are chart-local and bitmap-only. `Plot3D` stores an internal
  `RenderTargetBitmap` offscreen delegate (`Plot3D.cs:17`, `:344`), exposes
  `CaptureSnapshotAsync` and `SavePngAsync` (`Plot3D.cs:376`, `:466`), rejects
  non-PNG snapshot formats (`Plot3D.cs:382-385`), writes PNG via the snapshot
  path (`Plot3D.cs:427`, `:500`), and returns failed diagnostics instead of
  silent success when capture fails (`Plot3D.cs:452`). Output evidence reports
  `ImageExport` supported while `PdfExport` and `VectorExport` remain
  unsupported (`Plot3DOutputEvidence.cs:168-174`).

## Current Data Builders and Performance-Sensitive Paths

- Dense surface/waterfall data enters through `SurfaceMatrix`,
  `SurfacePyramidBuilder`, and `ISurfaceTileSource`. The builder accepts tile
  dimensions and a reduction kernel (`SurfacePyramidBuilder.cs:17-27`), tracks
  `LastBuildPeakScratchSampleCount` (`:43`, `:59`, `:73`), builds explicit LOD
  levels (`:51`, `:55-80`), and uses spans for source values, masks, color
  fields, and explicit-coordinate reduction (`:116`, `:118`, `:132`, `:237`,
  `:238`, `:258`).
- `InMemorySurfaceTileSource` is the current tile-serving hot path. It slices
  requested tiles from level matrices, returns `ValueTask<SurfaceTile?>`
  (`InMemorySurfaceTileSource.cs:90`, `:171`), uses span-backed source reads
  (`:115-117`), computes exact tile statistics (`:145`, `:153`, `:174-194`),
  and returns `null` for missing/invalid partitions rather than inventing data
  (`:96`, `:103`, `:109`).
- Surface rendering is tile/scene based: `SurfaceRenderer.BuildTile` consumes
  tile spans (`SurfaceRenderer.cs:28`, `:37`, `:39`), maps sample coordinates
  into render vertices (`:51-63`), and `BuildScene` composes render tiles
  (`:73-85`). Geometry generation is centralized in
  `SurfacePatchGeometryBuilder.Build` (`SurfacePatchGeometryBuilder.cs:6`,
  `:16`).
- LOD decisions have a real camera-aware estimator. `SurfaceScreenErrorEstimator`
  estimates data-window and tile projected footprints from metadata, data
  windows, tile bounds, and camera frames (`SurfaceScreenErrorEstimator.cs:7`,
  `:18`, `:37`, `:67`, `:101`).
- Scatter performance paths already separate point-list and columnar data.
  `ScatterChartData` exposes columnar series counts, total point counts,
  pickable points, streaming append/replace counts, FIFO capacity, and dropped
  point counts (`ScatterChartData.cs:58`, `:68`, `:73`, `:78`, `:83`, `:88`,
  `:93`, `:98`, `:103`, `:108`). Validation uses spans for columnar coordinates
  (`ScatterChartData.cs:146-150`).
- `DataLogger3D` wraps `ScatterColumnarSeries` for streaming-style cookbook
  paths. It exposes FIFO/append/replace/drop evidence (`DataLogger3D.cs:80`,
  `:85`, `:90`, `:95`, `:100`, `:105`), replaces/appends batches (`:122`,
  `:131`), and can switch to latest-window evidence without changing the
  retained source (`:141`, `:155`, `:164-175`).
- `ScatterRenderer` preserves the columnar shape into rendering: it allocates
  render-series slots for list and columnar series (`ScatterRenderer.cs:19`,
  `:25`) and builds columnar series directly from `X`, `Y`, and `Z` spans
  (`:52`, `:57-59`).
- Contour data has an extraction/cache path that can become expensive if Phase
  411 expands examples carelessly. `ContourSceneCache` caches extracted scenes
  (`ContourSceneCache.cs:4`, `:6`) and marching squares reads scalar/mask data
  through spans (`MarchingSquaresExtractor.cs:25`, `:44`).
- The demo currently exercises the performance-sensitive paths without claiming
  benchmarks: dense in-memory source creation uses `SurfacePyramidBuilder(32,
  32).Build(matrix)` (`samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs:830`,
  `:833`); analytics and waterfall proof paths create explicit
  `InMemorySurfaceTileSource` instances (`:836`, `:850`); scatter scenarios are
  selected through `ScatterStreamingScenarios` and default to
  `scatter-replace-100k` (`:137`, `:311-314`); the demo applies
  `Plot.Add.Surface`, `Plot.Add.Waterfall`, `Plot.Add.Scatter`,
  `Plot.Add.Bar`, and `Plot.Add.Contour` (`:371`, `:375`, `:398`, `:414`,
  `:430`).
- Demo diagnostics are evidence-only by construction. Scatter details report
  point counts, columnar counts, streaming appends, FIFO capacity, and dropped
  points (`MainWindow.axaml.cs:660-663`, `:703-708`); support summaries label
  values as evidence, not stable benchmark guarantees (`:1145`, `:1198`,
  `:1242`); rendering diagnostics expose fallback/native status explicitly
  (`:1479-1486`, `:1527-1536`).
- Current tests pin the native API and snapshot truth. Plot API tests assert
  typed returns for all add paths (`VideraChartViewPlotApiTests.cs:73-82`) and
  SavePng writes through the snapshot path (`:632`, `:649`). Snapshot tests
  assert empty plots, unsupported formats, and missing render hosts fail with
  diagnostics (`PlotSnapshotCaptureTests.cs:13`, `:26`, `:41`) and that image
  export is supported while PDF/vector export remain unsupported (`:101`,
  `:110`, `:119`).

## ScottPlot5-Inspired Ergonomics, Translated Natively

- Keep the inspiration narrow: short `Plot.Add.*` calls, returned handles,
  focused cookbook recipes, and high-performance example shapes. The current
  roadmap says Phase 409 translates ScottPlot5-inspired ergonomics into
  Videra-native concepts without compatibility claims
  (`.planning/ROADMAP.md:8-10`, `:37`).
- Native equivalents already exist:
  - concise add API: `chart.Plot.Add.Surface(...)`,
    `Waterfall(...)`, `Scatter(...)`, `Bar(...)`, `Contour(...)`;
  - returned handles: sealed `*Plot3DSeries` types with native kind/name
    identity;
  - axes: `chart.Plot.Axes.X/Y/Z`;
  - interactions: built-in orbit/dolly/pan/fit/reset routed through
    `SurfaceChartRuntime` and `VideraChartView`;
  - host-owned state: `ViewState`, pinned probes, draggable recipe values;
  - output: `Plot.SavePngAsync` / `CaptureSnapshotAsync` PNG only.
- Do not define API parity, adapter behavior, migration shims, or old
  `SurfaceChartView`/`WaterfallChartView`/`ScatterChartView` wrappers. The
  requirements explicitly reject ScottPlot compatibility adapters and migration
  shims (`.planning/REQUIREMENTS.md:62`, `:69`).

## Performance Truth Risks

- Fake benchmarks: cookbook snippets, screenshots, support summaries, and demo
  counters must not be described as throughput/FPS/latency benchmarks unless a
  real benchmark harness exists and is run. Current demo text already says
  evidence-only, not stable benchmark guarantees (`MainWindow.axaml.cs:1145`,
  `:1198`, `:1242`).
- Hidden fallback/downshift: failed cache-backed streaming must keep the
  previous plot path and report that no scenario/data-path fallback occurred
  (`MainWindow.axaml.cs:350-352`). Phase 411 must preserve explicit failure
  diagnostics rather than silently switching to smaller/simpler data.
- Unsupported backend promises: native backend support must remain the existing
  Videra boundary, not a SurfaceCharts performance promise. Requirements reject
  fallback/downshift and fake validation (`.planning/REQUIREMENTS.md:7-8`,
  `:55`, `:75`, `:77`), and roadmap Phase 411 requires truthful performance
  evidence with no unsupported backend promises (`.planning/ROADMAP.md:56-62`).
- Synthetic evidence: generated matrices, scatter scenarios, and contour fields
  are useful cookbook data shapes, but they are not production traces. Label
  them as generated demo data unless Phase 411 adds real captured fixtures.
- Snapshot truth: PNG snapshot success is not evidence of native backend
  performance. Snapshot export is chart-local bitmap output; PDF/vector remain
  unsupported by diagnostics (`Plot3DOutputEvidence.cs:168-174`).

## Candidate Minimal Phase 411 Slices

1. High-performance surface recipe slice: add one cookbook/demo recipe that
   shows a dense `SurfaceMatrix` -> `SurfacePyramidBuilder(maxTileWidth,
   maxTileHeight).Build(...)` -> `Plot.Add.Surface(...)` path, reports tile/LOD
   evidence only, and avoids benchmark language.
2. High-performance scatter recipe slice: add one recipe using `DataLogger3D`
   / `ScatterColumnarSeries` with configured FIFO capacity, visible-window
   evidence, retained/dropped counts, and `Plot.Add.Scatter(...)` returned
   handle usage.
3. No-hidden-fallback demo diagnostics slice: make the demo/cookbook wording
   consistently distinguish "path unavailable" from "downshifted to fallback"
   and keep the previous chart active on failure.
4. Performance evidence validation slice: add or tighten checks that reject
   benchmark-style language unless a real command-backed benchmark artifact is
   present.
5. Snapshot/performance separation slice: cookbook recipe can show
   `SavePngAsync`, but validation should ensure PNG snapshot docs are not used
   as rendering-performance proof.

## Candidate Validation Commands

Phase 411 should choose focused commands based on the files it touches. Candidate
commands from current evidence:

```powershell
dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~SurfacePyramidBuilderTests|FullyQualifiedName~InMemorySurfaceTileSourceTests|FullyQualifiedName~ScatterDataLogger3DTests|FullyQualifiedName~ScatterRendererTests"
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~RegressionGuardrailTests"
dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj
scripts/Test-SnapshotExportScope.ps1
rg -n "benchmark|FPS|throughput|fallback|downshift|OpenGL|WebGL|ScottPlot compatibility|adapter|shim" README.md docs samples src tests
```

Inventory-only Phase 409B validation:

```powershell
git diff --check -- .planning/phases/409-native-cookbook-and-ci-inventory/409B-NATIVE-PERFORMANCE-INVENTORY.md
git status --short
```

## Non-Goals and Anti-Fake Constraints

- No product, demo, test, README/docs, `.beads`, roadmap/state, or sibling
  phase-file edits in this Phase 409B bead.
- No ScottPlot API parity, compatibility wrappers, migration adapters, or
  reintroduced old chart controls.
- No generic plotting engine, backend expansion, OpenGL/WebGL promise,
  compositor-native Wayland promise, PDF/vector export, or hidden output
  fallback.
- No fake performance proof from generated demo data, screenshots, support
  text, skipped tests, or successful PNG snapshots.
- No synthetic pass/fail claims: validation must name the exact command and
  result, or label the item as not run.

## Blockers

No technical blocker was found for Phase 411 planning. The main risk is scope
creep: performance examples should stay as small native Videra cookbook/data
path slices until a real benchmark harness is explicitly added.
