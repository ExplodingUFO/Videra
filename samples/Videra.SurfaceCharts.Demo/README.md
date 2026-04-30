# Videra.SurfaceCharts.Demo

`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.

For the canonical surface/cache-backed chart story, start from `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing`, and use this demo as the repository reference app for the paths it actually exposes. `VideraChartView` ships in `Videra.SurfaceCharts.Avalonia`, and this sample exercises `Plot.Add.Surface`, `Plot.Add.Waterfall`, `Plot.Add.Scatter`, `Plot.Add.Bar`, and `Plot.Add.Contour` on that one control.

The shipped efficiency story is the same chart-local path used by the benchmark gate: tighter interactive residency under camera movement and lower probe-path churn.
The scatter proof also exposes columnar streaming/FIFO truth from `ScatterColumnarSeries`: retained point count, append/replacement batch count, configured FIFO capacity, FIFO dropped point count, pickable point count, and Plot.Add.Scatter `InteractionQuality`. The demo source uses deterministic `ScatterStreamingScenarios` for replace, append, and FIFO-trim paths through `ReplaceRange(...)` / `AppendRange(...)`, optional `fifoCapacity`, and the high-volume default `Pickable=false`.

The sample stays separate from `Videra.Demo` and `VideraView`. It exercises the chart-local renderer seam shipped in `VideraChartView`, not a `VideraView` mode. It provides switchable sources, built-in chart interaction, and a bounded cookbook gallery:

- `Start here: In-memory first chart`: builds a sample surface matrix at startup and feeds it through `SurfacePyramidBuilder`.
- `Explore next: Cache-backed streaming`: loads manifest metadata from `Assets/sample-surface-cache/sample.surfacecache.json`, then uses lazy tile streaming from `Assets/sample-surface-cache/sample.surfacecache.json.bin` through `SurfaceCacheReader` and `SurfaceCacheTileSource`.
- `Try next: Analytics proof`: exercises an explicit-coordinate `VideraChartView` path with independent scalar `ColorField` and pinned-probe workflow proof data.
- `Try next: Waterfall proof`: exercises the thin `VideraChartView` proof on the same Avalonia shell.
- `Try next: Scatter proof`: exercises the repo-owned scatter proof path on the same Avalonia shell with direct point-object data plus selectable deterministic columnar replace, append, and FIFO-trim scenarios.
- `Try next: Bar chart proof`: exercises the bounded grouped-bar proof path on the same Avalonia shell.
- `Try next: Contour plot proof`: exercises the bounded contour proof path on the same Avalonia shell.
- `Cookbook gallery`: maps first chart, styling, interactions, live data, linked axes, Bar, Contour, and export recipes to isolated setup paths and matching snippets without becoming a generic chart editor.

The cookbook is recipe-first and native to Videra. It is not a compatibility or parity layer: the snippets use Videra's 3D `VideraChartView`, `Plot.Add.*`, `Plot.Axes`, `DataLogger3D`, linked-view, and chart-local PNG snapshot APIs.
For current package consumption, migration notes, non-goals, support artifacts, and troubleshooting, use [SurfaceCharts Current Consumer Handoff](../../docs/surfacecharts-release-cutover.md). The handoff is documentation only and does not approve publishing, tagging, or GitHub Release publication. The older [SurfaceCharts Release Candidate Handoff](../../docs/surfacecharts-release-candidate-handoff.md) remains release-candidate background.

VideraChartView exposes `ViewState` as the chart-view contract for persisted camera and data-window state.
VideraChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, `Ctrl + left-drag` focus zoom, and `Shift + left-click` pinned probe on top of the `ViewState` runtime contract.
The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.
The public interaction diagnostics are `InteractionQuality` + `InteractionQualityChanged` with `Interactive` / `Refine`.
The public overlay configuration seam is `SurfaceChartOverlayOptions` through `Plot.OverlayOptions`; overlay state types remain internal.
Hosts own `ISurfaceTileSource`, persisted `ViewState`, color-map selection, and chart-local product UI.
`VideraChartView` owns chart-local built-in gestures, tile scheduling/cache, overlay presentation, native-host/render-host orchestration, and `RenderingStatus` projection.
The public rendering truth is `RenderingStatus` + `RenderStatusChanged` with `ActiveBackend`, `IsReady`, `IsFallback`, `FallbackReason`, `UsesNativeSurface`, `ResidentTileCount`, `VisibleTileCount`, and `ResidentTileBytes`.
The `Videra.SurfaceCharts.*` family is part of the current public package promise. The public chart package line is `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing` for the surface/cache-backed path. `Videra.SurfaceCharts.Avalonia` brings `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` transitively. This demo remains repository-only.

The committed cache sample uses a tiled manifest+sidecar layout so panning, dolly, and focus changes request different cache tiles instead of materializing every tile value into memory up front.

## Start Here

1. Run the sample and keep the default `Start here: In-memory first chart` source.
2. Confirm the baseline first chart renders, then try `FitToData()`, `ResetCamera()`, orbit, pan, dolly, and focus zoom.
3. Move to `Explore next: Cache-backed streaming` only after the first chart path works and you want to validate lazy tile reads plus the broader demo surfaces.
4. Use `Try next: Analytics proof` to validate explicit-coordinate sampling and independent color scalar flow while keeping `VideraChartView` as the proof host.
5. Use `Try next: Waterfall proof` when you want the second chart-family proof on the same Avalonia shell.
6. Use `Try next: Scatter proof` when you want the Plot.Add.Scatter proof and columnar data-path diagnostics on the same Avalonia shell. Use the `Scatter stream` selector to switch between replace, append, and FIFO-trim scenarios.
7. Use `Try next: Bar chart proof` when you want the grouped-bar proof path without opening a generic editor.
8. Use `Try next: Contour plot proof` when you want the radial scalar-field contour proof path without opening a generic editor.

## Run

```bash
dotnet run --project samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj
```

## Cookbook Recipes

The demo's `Cookbook gallery` selector groups recipes by intent. Selecting a recipe switches to a bounded visible result when the result already exists in the sample shell, and the visible snippet matches the documentation below.

Detailed copy-adapt recipes live beside this README:

- [First chart](Recipes/first-chart.md)
- [Surface and cache-backed data](Recipes/surface-cache-backed.md)
- [Waterfall](Recipes/waterfall.md)
- [Axes and linked views](Recipes/axes-and-linked-views.md)
- [Scatter and live data](Recipes/scatter-and-live-data.md)
- [Bar](Recipes/bar.md)
- [Contour](Recipes/contour.md)
- [Support evidence](Recipes/support-evidence.md)
- [PNG snapshot](Recipes/png-snapshot.md)

## Demo Coverage Map

| Visible scenario | Native path | Recipe evidence | Focused tests |
| --- | --- | --- | --- |
| `Start here: In-memory first chart` | `Plot.Add.Surface` with generated matrix data | `Recipes/first-chart.md` | `SurfaceChartsDemoViewportBehaviorTests`, `SurfaceChartsCookbookFirstSurfaceRecipeTests` |
| `Explore next: Cache-backed streaming` | `Plot.Add.Surface` with committed cache manifest and tile payload | `Recipes/surface-cache-backed.md` | `SurfaceChartsDemoViewportBehaviorTests`, `SurfaceChartsHighPerformancePathTests` |
| `Try next: Analytics proof` | `Plot.Add.Surface` with explicit coordinates and `ColorField` | `Recipes/first-chart.md` | `SurfaceChartsDemoViewportBehaviorTests` |
| `Try next: Waterfall proof` | `Plot.Add.Waterfall` with explicit strip spacing | `Recipes/waterfall.md`, `Recipes/axes-and-linked-views.md` | `SurfaceChartsDemoViewportBehaviorTests`, `SurfaceChartsCookbookWaterfallLinkedRecipeTests` |
| `Try next: Scatter proof` | `Plot.Add.Scatter` with descriptor-scoped streaming scenarios | `Recipes/scatter-and-live-data.md` | `SurfaceChartsDemoViewportBehaviorTests`, `ScatterStreamingScenarioEvidenceTests` |
| `Try next: Bar chart proof` | `Plot.Add.Bar` with grouped series, category labels, and `SetSeriesColor` | `Recipes/bar.md` | `SurfaceChartsCookbookBarContourSnapshotRecipeTests`, `VideraChartViewPlotApiTests` |
| `Try next: Contour plot proof` | `Plot.Add.Contour` with radial scalar-field levels and explicit level overloads | `Recipes/contour.md` | `SurfaceChartsCookbookBarContourSnapshotRecipeTests`, `VideraChartViewPlotApiTests` |
| `Built-in interaction` | `TryCreateProbeAnnotationAnchor`, `TryCreateSelectionMeasurementReport`, and `SelectionReported` report surfaces | `Recipes/axes-and-linked-views.md` | `SurfaceChartInteractionRecipeTests`, `SurfaceChartInteractionTests` |
| `Capture Snapshot` | `Plot.SavePngAsync` / `CaptureSnapshotAsync` PNG manifest evidence | `Recipes/png-snapshot.md`, `Recipes/support-evidence.md` | `PlotSnapshotContractTests`, `PlotSnapshotCaptureTests`, `SurfaceChartsDemoViewportBehaviorTests` |

### First Chart

First surface from a small matrix:

```csharp
var chart = new VideraChartView();

chart.Plot.Add.Surface(new double[,]
{
    { 0.0, 0.4, 0.8 },
    { 0.2, 0.7, 1.0 },
    { 0.1, 0.5, 0.9 },
}, "First surface");

chart.Plot.Axes.X.Label = "Time";
chart.Plot.Axes.X.Unit = "s";
chart.Plot.Axes.Y.Label = "Height";
chart.Plot.Axes.Y.Unit = "mm";
chart.Plot.Axes.Z.Label = "Band";
chart.Plot.Axes.Z.Unit = "Hz";
chart.FitToData();
```

### Styling

Axes, color map, and chart-local overlay options:

```csharp
chart.Plot.Axes.X.Label = "Time";
chart.Plot.Axes.X.Unit = "s";
chart.Plot.Axes.Y.Label = "Height";
chart.Plot.Axes.Y.Unit = "mm";
chart.Plot.Axes.Z.Label = "Band";
chart.Plot.Axes.Z.Unit = "Hz";
chart.Plot.ColorMap = new SurfaceColorMap(
    new SurfaceValueRange(0, 1),
    SurfaceColorMapPresets.CreateProfessional());
chart.Plot.OverlayOptions = new SurfaceChartOverlayOptions
{
    ShowMinorTicks = true,
};
```

### Interactions

Bounded interaction profile and host command wiring:

```csharp
chart.InteractionProfile = new SurfaceChartInteractionProfile
{
    IsOrbitEnabled = true,
    IsPanEnabled = true,
    IsDollyEnabled = true,
    IsProbePinningEnabled = true,
};

chart.TryExecuteChartCommand(SurfaceChartCommand.FitToData);
chart.TryResolveProbe(pointerPosition, out var probe);
```

### Live Data

First scatter from coordinate arrays:

```csharp
chart.Plot.Clear();
chart.Plot.Add.Scatter(
    x: [0.0, 1.0, 2.0, 3.0],
    y: [0.1, 0.5, 0.9, 0.4],
    z: [0.0, 0.3, 0.6, 1.0],
    name: "First scatter");
chart.FitToData();
```

Live scatter through `DataLogger3D`:

```csharp
var live = new DataLogger3D(0xFF2F80EDu, label: "Live scatter", fifoCapacity: 10_000);
live.Append(new ScatterColumnarData(
    new float[] { 0f, 1f, 2f },
    new float[] { 0.2f, 0.5f, 0.8f },
    new float[] { 0f, 0.5f, 1f }));

var liveData = new ScatterChartData(
    new ScatterChartMetadata(
        new SurfaceAxisDescriptor("Time", "s", 0, 10),
        new SurfaceAxisDescriptor("Band", "Hz", 0, 10),
        new SurfaceValueRange(0, 1)),
    [],
    [live.Series]);

chart.Plot.Clear();
chart.Plot.Add.Scatter(liveData, "Live scatter");
```

Latest-window evidence for live views:

```csharp
live.UseLatestWindow(2_000);
var evidence = live.CreateLiveViewEvidence();
```

### Linked Axes

Explicit two-chart view linking with a disposable lifetime:

```csharp
var left = new VideraChartView();
var right = new VideraChartView();

left.Plot.Add.Surface(new double[,]
{
    { 0.0, 0.4, 0.8 },
    { 0.2, 0.7, 1.0 },
    { 0.1, 0.5, 0.9 },
}, "Left");
right.Plot.Add.Surface(new double[,]
{
    { 0.1, 0.5, 0.9 },
    { 0.3, 0.8, 1.1 },
    { 0.2, 0.6, 1.0 },
}, "Right");

using var link = left.LinkViewWith(right);
left.Plot.Axes.X.SetBounds(0, 10);
right.FitToData();
```

### Bar

Grouped bars from three named series:

```csharp
var data = new BarChartData(
[
    new BarSeries([12.0, 19.0, 3.0, 5.0, 8.0], 0xFF38BDF8u, "Series A"),
    new BarSeries([7.0, 11.0, 15.0, 8.0, 13.0], 0xFFF97316u, "Series B"),
    new BarSeries([5.0, 9.0, 12.0, 18.0, 6.0], 0xFF2DD4BFu, "Series C"),
], categoryLabels: ["Q1", "Q2", "Q3", "Q4", "Q5"]);

chart.Plot.Clear();
var bars = chart.Plot.Add.Bar(data, "Grouped bars");
bars.SetSeriesColor(seriesIndex: 1, color: 0xFFABCDEFu);
chart.FitToData();
```

### Contour

Contour lines from a small radial scalar field:

```csharp
const int size = 32;
var values = new float[size * size];
for (var y = 0; y < size; y++)
{
    for (var x = 0; x < size; x++)
    {
        var dx = (x - (size - 1) / 2.0) / ((size - 1) / 2.0);
        var dy = (y - (size - 1) / 2.0) / ((size - 1) / 2.0);
        values[y * size + x] = (float)Math.Sqrt(dx * dx + dy * dy);
    }
}

var range = new SurfaceValueRange(values.Min(), values.Max());
var field = new SurfaceScalarField(size, size, values, range);

chart.Plot.Clear();
chart.Plot.Add.Contour(field, explicitLevels: [0.25f, 0.5f, 0.75f], name: "Radial contours");
chart.FitToData();
```

### Export

Save the active plot as PNG:

```csharp
var result = await chart.Plot.SavePngAsync(
    "artifacts/surfacecharts/first-chart.png",
    width: 1920,
    height: 1080);

if (!result.Succeeded)
{
    throw new InvalidOperationException(result.Failure?.Message);
}
```

## What The Demo Shows Today

- an independent chart application boundary
- a `Start here` in-memory first chart path
- a cookbook/gallery navigation model with first chart, styling, interactions, live data, linked axes, and export groups
- an `Explore next` cache-backed streaming path
- a `Try next` analytics proof with explicit/non-uniform coordinates and independent `ColorField` on `VideraChartView`
- a `Try next` waterfall proof path
- a `Try next` scatter proof path with direct point-object data and deterministic columnar replace, append, and FIFO-trim scenarios
- a `Try next` bar chart proof path with grouped `BarChartData`
- a `Try next` contour plot proof path with radial `ContourChartData`
- built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + left-drag` focus zoom on the surface and waterfall proof paths; the scatter proof keeps left-drag orbit and wheel dolly only
- a `View-state contract` panel that projects `ViewState`, `FitToData()`, `ResetCamera()`, and `ZoomTo(...)`
- an `Interaction quality` panel that projects `Interactive` and `Refine`
- overview-first LOD behavior
- lazy cache-backed tile reads through the committed `sample.surfacecache.json` and `.bin` sidecar
- axis/legend overlays rendered by `VideraChartView`
- an `Overlay options` panel that shows chart-local `Plot.OverlayOptions` for formatter, minor ticks, grid plane, and axis-side behavior
- hover readout and `Shift + left-click` pinned probes on the chart surface
- the shipped `GPU-first` renderer path used by `VideraChartView`, with `software fallback` still available when native-host or GPU initialization is unavailable
- a lightweight rendering-path panel driven by `RenderingStatus` / `RenderStatusChanged`
- a dedicated `RenderingStatus` diagnostics panel that exposes the active backend, readiness, fallback status/reason, native-host state, resident tile count, visible tile count, and resident tile bytes for the active surface chart path
- scatter diagnostics for series count, point count, columnar series count, retained columnar point count, append/replacement batch count, FIFO dropped points, configured FIFO capacity, pickable point count, and `InteractionQuality`; the columnar high-volume path defaults to `Pickable=false`
- a copyable evidence-only support summary that includes active chart control type, runtime/assembly identity, backend/display environment variables, last cache-load failure for the cache-backed path (no Plot path switch on failure), `SeriesCount`, `ActiveSeries`, `ChartKind`, `ColorMap`, `PrecisionProfile`, `OutputEvidenceKind`, `OutputCapabilityDiagnostics`, `DatasetEvidenceKind`, `DatasetSeriesCount`, `DatasetActiveSeriesIndex`, `DatasetActiveSeriesMetadata`, selected scenario id/name/update mode, point counts, FIFO capacity, pickability, rendering status, and dataset/Plot path details; Cache-backed data path unavailable and there is no scenario/data-path fallback. The previous chart Plot path remains active on load failure; these values are support evidence, not stable benchmark guarantees, image/PDF/vector export, or backend fallback proof. For packaged smoke handoff, collect `consumer-smoke-result.json`, `diagnostics-snapshot.txt`, `surfacecharts-support-summary.txt`, `chart-snapshot.png`, and trace/stdout/stderr/environment logs.

Maintainers can include the deterministic scatter streaming scenarios in the repo-owned visual evidence bundle:

```powershell
pwsh -File ./scripts/Invoke-PerformanceLabVisualEvidence.ps1 -Configuration Release -OutputRoot artifacts/performance-lab-visual-evidence
```

The bundle includes PNG visual evidence, manifest JSON, summary text, and per-scenario diagnostics text for PR review or support reports. It is evidence-only, not a pixel-perfect visual-regression gate, not a stable benchmark guarantee, and not a claim that SurfaceCharts has gained GPU-driven culling or a new chart family.

## What The Demo Does Not Show Yet

The current sample is still a focused onboarding surface, not a finished end-user chart workstation:

- no exhaustive UI that forces every GPU host / fallback combination; the diagnostics panel reports the active runtime path and current `RenderingStatus` fields
- no replacement for `VideraDiagnosticsSnapshotFormatter`; the SurfaceCharts support summary stays separate and reports only chart-local evidence from this demo path
- no hard performance guarantee from the demo scenarios; benchmark thresholds remain in the dedicated benchmark gate files after CI history supports promotion
- no GPU-driven culling, renderer rewrite, new chart family, or generic benchmark editor
- on Linux, native GPU hosting still depends on X11 handles; Wayland sessions are `XWayland compatibility` only, not compositor-native Wayland surface embedding

That gap is intentional. The current demo primarily proves the control boundary, built-in interaction contract, renderer path, active fallback diagnostics, probe/overlay behavior, and cache/LOD story without trying to be a full workstation shell.
