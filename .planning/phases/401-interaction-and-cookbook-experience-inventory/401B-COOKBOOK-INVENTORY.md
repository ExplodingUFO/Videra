---
phase: 401
title: "Cookbook Demo Surface Inventory"
bead: Videra-nye
scope: "SurfaceCharts demo and docs cookbook readiness only"
status: inventory
---

# 401B Cookbook Inventory

## Scope

This inventory is read-only evidence for the SurfaceCharts demo and cookbook
documentation surface. It does not propose product runtime changes. The Phase
403 candidates below are intentionally small slices that can be implemented
independently from interaction/code-experience work if write sets stay
disjoint.

Inspected surfaces:

- `samples/Videra.SurfaceCharts.Demo/`
- `samples/Videra.SurfaceCharts.Demo/README.md`
- `README.md`
- `docs/surfacecharts-release-cutover.md`
- `docs/surfacecharts-release-candidate-handoff.md`
- `docs/index.md`
- `docs/zh-CN/README.md`
- `docs/zh-CN/modules/videra-surfacecharts-*.md`
- `.planning/phases/388-cookbook-demo-gallery-and-docs/`
- `.planning/milestones/v2.55-phases/381-cookbook-demo-and-docs/381-VERIFICATION.md`

## Current Demo Entry Points

The repository-only demo is `samples/Videra.SurfaceCharts.Demo`, an Avalonia
`net8.0` WinExe with project references to
`src/Videra.SurfaceCharts.Avalonia` and
`src/Videra.SurfaceCharts.Processing`. It copies
`Assets/sample-surface-cache/**` into the output.

Run command:

```bash
dotnet run --project samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj
```

Visible chart views in `Views/MainWindow.axaml`:

- `ChartView`
- `WaterfallPlotView`
- `ScatterPlotView`
- `BarChartPlotView`
- `ContourPlotView`

Visible source selector entries:

- `Start here: In-memory first chart`
- `Explore next: Cache-backed streaming`
- `Try next: Analytics proof`
- `Try next: Waterfall proof`
- `Try next: Scatter proof`
- `Try next: Bar chart proof`
- `Try next: Contour plot proof`

Cookbook gallery entries in `Views/MainWindow.axaml.cs`:

- `First chart: Surface from a matrix`
- `Styling: Axes, color map, and overlay options`
- `Interactions: Profile plus bounded commands`
- `Live data: Latest-window scatter stream`
- `Linked axes: Explicit two-chart view link`
- `Export: Chart-local PNG snapshot`

Support and validation UI already present:

- `Copy recipe snippet`
- `Fit to data`
- `Reset camera`
- rendering path/status panels
- probe workflow text
- overlay options text
- dataset summary
- `Copy support summary`
- `Capture Snapshot` through `Plot.SavePngAsync(...)`

## Current Docs Entry Points

Public/repository docs already point users to the cookbook path:

- `README.md` has `Minimal SurfaceCharts cookbook`, package install guidance,
  a compact first chart + live scatter + PNG snippet, and a link to the demo
  README for more snippets.
- `samples/Videra.SurfaceCharts.Demo/README.md` documents the run command,
  cookbook groups, first chart, styling, interactions, live data, linked axes,
  export, support summary, and demo non-goals.
- `docs/surfacecharts-release-cutover.md` is the current consumer-facing
  package/cookbook/migration/support handoff and lists the six cookbook groups.
- `docs/surfacecharts-release-candidate-handoff.md` preserves the older
  release-candidate package/cookbook/support route.
- `docs/index.md`, `docs/package-matrix.md`, `docs/releasing.md`, and
  `docs/troubleshooting.md` link to the cutover/cookbook path.
- Chinese docs identify the SurfaceCharts module family, repository-only demo,
  package split, and demo support-ready route.

Prior phase evidence:

- Phase 381 verified cookbook-style root/demo README recipes for concise
  `Plot.Add`, `Plot.Axes`, `SavePngAsync`, and live scatter usage while keeping
  the demo bounded.
- Phase 388 added the bounded `Cookbook gallery` panel, the six recipe groups,
  copyable snippets, root/demo README cookbook sections, and focused
  docs/demo contract tests.

## Recipe and Copyability Gaps

1. Bar/Contour proof paths are visible in the current demo but are not
   represented in the demo README's top entry-point list, `Start Here` steps,
   cookbook recipes, release cutover cookbook list, or root cookbook summary.
   This repeats the lag already called out in Phase 390 release-readiness
   notes.
2. The six cookbook recipes still describe the Phase 388 surface/waterfall/
   scatter-centered set. They do not yet cover current public chart families
   `Plot.Add.Bar(...)` and `Plot.Add.Contour(...)` even though the demo exposes
   both through source selector entries and support summaries.
3. Several snippets are illustrative but not fully copyable as standalone
   first-use samples:
   - `Interactions` references `pointerPosition` without showing how the host
     obtains it.
   - `Live Data` in the demo snippet references `x`, `y`, `z`, and
     `scatterData`; the README has a fuller example but the UI copy does not.
   - `Linked Axes` references `surfaceSource` and `comparisonSource` without a
     minimal source construction path.
4. The root README says "More cookbook snippets" live in the demo README, but
   users still need to stitch together package install, using directives,
   sample data construction, and control hosting details across multiple
   sections.
5. The demo can copy a recipe snippet, but there is no obvious automated check
   that every visible source selector entry has matching README coverage or a
   cookbook recipe when appropriate.
6. The release cutover support route mentions `Start here`, cache-backed,
   analytics, waterfall, and scatter repro paths, but not the currently visible
   bar/contour paths.

## Minimal Candidate Slices for Phase 403

### Slice 403A: Align Docs With Current Demo Paths

Update only docs/README surfaces so the current visible demo entry points are
truthful.

Candidate writes:

- `samples/Videra.SurfaceCharts.Demo/README.md`
- `README.md`
- `docs/surfacecharts-release-cutover.md`
- possibly `docs/surfacecharts-release-candidate-handoff.md` if the older page
  must remain aligned

Acceptance:

- Bar and contour proof paths are mentioned wherever the demo path list is
  enumerated.
- The cookbook route still states that `Videra.SurfaceCharts.Demo` is
  repository-only.
- ScottPlot 5 remains inspiration only, with no compatibility/parity language.

### Slice 403B: Add Bar and Contour Cookbook Recipes

Add two bounded cookbook recipe entries and matching README snippets for
`Plot.Add.Bar(...)` and `Plot.Add.Contour(...)`.

Candidate writes:

- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`
- `samples/Videra.SurfaceCharts.Demo/README.md`
- focused docs/demo contract tests

Acceptance:

- `CookbookRecipeSelector` exposes Bar and Contour recipes.
- Selecting each recipe switches to the existing proof path instead of adding a
  generic editor.
- Snippets are short, use current public APIs, and do not depend on demo-only
  private helper methods.

### Slice 403C: Make Existing Snippets More Self-Contained

Tighten the existing copyable snippets without changing product APIs.

Candidate writes:

- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`
- `samples/Videra.SurfaceCharts.Demo/README.md`
- focused docs/demo contract tests

Acceptance:

- `Live data` snippet includes minimal arrays and `ScatterChartData`
  construction, or explicitly links to the fuller README snippet.
- `Linked axes` snippet includes a minimal source construction path or clearly
  labels `surfaceSource` / `comparisonSource` as caller-owned sources.
- `Interactions` snippet avoids unexplained variables or labels them as host
  pointer input.

### Slice 403D: Add Demo/Docs Coverage Guard

Add a focused repository test or grep check that prevents the UI/docs drift
found above from recurring.

Candidate writes:

- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs`

Acceptance:

- Every `SourceSelector` proof entry in XAML has corresponding demo README
  wording.
- Every cookbook recipe group expected by README/cutover docs is present in the
  demo code.
- Test remains text-contract focused and does not run the Avalonia UI.

## Non-Goals and Fallback Exclusions

Keep Phase 403 out of these areas unless a separate bead explicitly changes the
boundary:

- no ScottPlot compatibility layer, adapter, parity promise, or migration shim
- no old `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`
  controls
- no direct public `VideraChartView.Source` API
- no PDF/vector export and no image export beyond bounded PNG chart snapshots
- no backend expansion, OpenGL/WebGL addition, or compositor-native Wayland
  promise
- no hidden scenario/data-path fallback; failed cache-backed loading should
  report failure while the previous Plot path remains active
- no generic chart editor, plotting engine, product workbench, or god-code demo
  shell
- no benchmark/performance guarantee from cookbook snippets or support
  summaries
- no package dependency on `Videra.SurfaceCharts.Demo`; it remains
  repository-only

## Validation Commands

Inventory-only validation:

```bash
git status --short
```

Focused Phase 403 validation candidates:

```bash
dotnet restore tests/Videra.Core.Tests/Videra.Core.Tests.csproj
dotnet restore samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|SurfaceChartsDemoViewportBehaviorTests" --no-restore
dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj --no-restore
pwsh -NoProfile -File scripts/Test-SnapshotExportScope.ps1
git diff --check
git status --short
```

If Phase 403 touches only docs and text-contract tests, the focused
`SurfaceChartsDemoConfigurationTests` slice plus `git diff --check` should be
the minimum gate. If it touches demo XAML/code-behind, also run the demo build
and viewport behavior tests.
