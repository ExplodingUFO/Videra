# Videra.SurfaceCharts.Avalonia

`Videra.SurfaceCharts.Avalonia` provides the dedicated Avalonia control surface for the surface-chart module family.

The control layer remains separate from `VideraView` and only depends on the shared surface-chart contracts in `Videra.SurfaceCharts.Core`.

`nuget.org` is the default public feed for this package. `GitHub Packages` remains `preview` / internal validation only. The current support level is `alpha`. Add `Videra.SurfaceCharts.Processing` only for the surface/cache-backed path.

The current shipped surface path is tighter under camera movement and churns less on probe work, without widening the existing chart-local path.

VideraChartView exposes `ViewState` as the chart-view contract for persisted camera and data-window state.
VideraChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + Left drag` focus zoom on top of the `ViewState` runtime contract.
The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.
The public interaction diagnostics are `InteractionQuality` + `InteractionQualityChanged` with `Interactive` / `Refine`.
`VideraChartView` follows the same chart-local terminology on the direct scatter path: left-drag navigation reports `Interactive`, release/capture loss returns to `Refine`, and `ScatterChartRenderingStatus` carries `InteractionQuality` alongside retained columnar streaming counters. Columnar scatter data still comes from `ScatterColumnarSeries` through `ReplaceRange(...)` / `AppendRange(...)`, optional `fifoCapacity`, and the high-volume default `Pickable=false`.
Hosts can keep professional axis, grid, and legend behavior chart-local through `VideraChartView.Plot.OverlayOptions` for formatter, title/unit override, minor ticks, grid plane, and axis-side selection.
The public overlay configuration seam is `SurfaceChartOverlayOptions` through `Plot.OverlayOptions`; overlay state types remain internal.

## Current Scope

`VideraChartView` currently provides:

- a chart-local renderer seam through `SurfaceChartRenderHost`; it is not a `VideraView` mode
- one Plot authoring surface with `Plot.Add.Surface`, `Plot.Add.Waterfall`, and `Plot.Add.Scatter`
- a `GPU-first` renderer path with an explicit chart-local `software fallback` seam (no viewer/backend downshift)
- control-visible `RenderingStatus` / `RenderStatusChanged` truth for `ActiveBackend`, `IsReady`, `IsFallback`, `FallbackReason`, `UsesNativeSurface`, `ResidentTileCount`, `VisibleTileCount`, and `ResidentTileBytes`
- host-driven surface rendering from an `ISurfaceTileSource`
- `ViewState` as the chart-view contract for persisted camera and data-window state
- host-driven `FitToData()`, `ResetCamera()`, and `ZoomTo(...)` commands
- built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + Left drag` focus zoom
- explicit `InteractionQuality` / `InteractionQualityChanged` diagnostics with `Interactive` and `Refine` interaction-quality states
- `VideraChartView` render-status diagnostics for `InteractionQuality`, columnar retained point count, append/replacement batch count, FIFO dropped points, configured FIFO capacity, and pickable point count
- public overlay configuration through `SurfaceChartOverlayOptions` / `Plot.OverlayOptions`; overlay state types remain internal
- chart-local `Plot.OverlayOptions` for formatter, title/unit override, minor ticks, grid plane, and axis-side selection
- overview-first tile scheduling with lazy cache-backed reads
- color-map driven surface rendering
- chart-local axis/legend overlays and hover/pinned probe readout, including `Shift + LeftClick` pinning
- `VideraChartView` owns chart-local built-in gestures, tile scheduling/cache, overlay presentation, native-host/render-host orchestration, and `RenderingStatus` projection

`VideraChartView` is the single shipped chart control on top of the chart shell.
The scatter path is intentionally direct and chart-local: it does not introduce `ViewState` or `VideraView` semantics, and its columnar streaming/FIFO diagnostics remain on `ScatterChartRenderingStatus`.

This module is intentionally a thin UI shell. Tile decoding, preprocessing, cache generation, and LOD policy remain outside the control layer.

## Renderer Truth

- `VideraChartView` works through a chart-local renderer seam and stays independent from `VideraView`.
- The renderer is `GPU-first`, but chart-local `software fallback` remains a shipped path for unsupported or fallback-triggering environments.
- Hosts can inspect `RenderingStatus` and subscribe to `RenderStatusChanged` instead of relying on silent backend switches.
- Linux native GPU hosting currently embeds through X11 handles. On Wayland sessions the chart host uses an `XWayland compatibility` path; compositor-native Wayland surface embedding is not available in this host shell today.

## Current Limitations

- exhaustive native-host validation still depends on the host/platform combination; Linux Wayland validation remains limited to the documented XWayland compatibility path
- the current public demo stays intentionally narrow and does not expose every renderer diagnostic or cache-control knob

## Minimal Usage

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:charts="using:Videra.SurfaceCharts.Avalonia.Controls">
    <charts:VideraChartView x:Name="ChartView" />
</Window>
```

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

var matrix = new SurfaceMatrix(
    new SurfaceMetadata(
        width: 256,
        height: 128,
        new SurfaceAxisDescriptor("Time", "s", 0d, 255d),
        new SurfaceAxisDescriptor("Frequency", "Hz", 0d, 127d),
        new SurfaceValueRange(-1d, 1d)),
    sampleValues);

var chartView = new VideraChartView
{
    Source = new SurfacePyramidBuilder(32, 32).Build(matrix),
    ViewState = SurfaceViewState.CreateDefault(
        matrix.Metadata,
        new SurfaceDataWindow(0d, 0d, matrix.Metadata.Width, matrix.Metadata.Height)),
};

chartView.Plot.Add.Surface(chartView.Source, "Surface");
chartView.Plot.ColorMap = colorMap;
chartView.Plot.OverlayOptions = new SurfaceChartOverlayOptions
{
    ShowMinorTicks = true,
    GridPlane = SurfaceChartGridPlane.XZ,
    LabelFormatter = static (_, value) => value.ToString("0.##")
};

chartView.ZoomTo(new SurfaceDataWindow(64d, 32d, 128d, 96d));
chartView.ResetCamera();
```

### Professional Presentation And Precision

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

var matrix = ...;
var source = new SurfacePyramidBuilder(32, 32).Build(matrix);

var chartView = new VideraChartView { Source = source };
chartView.Plot.Add.Surface(source, "Surface");
chartView.Plot.ColorMap = new SurfaceColorMap(matrix.Metadata.ValueRange, SurfaceColorMapPresets.CreateProfessional());
chartView.Plot.OverlayOptions = SurfaceChartOverlayPresets.Professional;

chartView.Plot.OverlayOptions = SurfaceChartNumericLabelPresets.Engineering(precision: 4);
chartView.Plot.OverlayOptions = SurfaceChartNumericLabelPresets.Scientific(precision: 2);
chartView.Plot.OverlayOptions = SurfaceChartNumericLabelPresets.Fixed(precision: 1);
chartView.Plot.OverlayOptions = SurfaceChartOverlayPresets.Compact;
```

`SurfaceColorMapPresets` exposes `CreateDefault()`, `CreateProfessional()`, `CreateCoolWarm()`, and `CreateGrayscale()` entry palettes for quick, proof-ready chart setups.
`SurfaceChartNumericLabelPresets` keeps numeric precision explicit per overlay mode (`Engineering`, `Scientific`, `Fixed`) and `SurfaceChartOverlayPresets` provides tuned `Professional` and `Compact` chart profiles.
`SurfaceChartEvidenceFormatter.Create(...)` provides deterministic chart-local output evidence for support/adoption surfaces that need palette name, color stops, precision profile, and representative formatted labels without coupling to image export, file I/O, viewer diagnostics, or a renderer backend.
`SurfaceChartProbeEvidenceFormatter.Create(...)` and `SurfaceChartProbeEvidenceFormatter.Format(...)` provide chart-local probe evidence for hovered and pinned `SurfaceProbeInfo` values plus `SurfaceChartOverlayOptions`; overlay state types remain internal, and the report does not add `VideraView` or viewer-runtime semantics.

`SurfaceMatrix` remains the simplest source-first regular-grid entrypoint. When you need richer analytics payloads, you can keep the same `VideraChartView` shell and switch the underlying source construction to `SurfaceScalarField`, an independent `ColorField`, and `SurfaceMask` without widening `VideraChartView` itself.

Hosts currently own:

- `ISurfaceTileSource` creation
- persisted `ViewState` updates for camera and data-window state
- color-map selection
- any chart-local product UI layered on top of the built-in orbit / pan / dolly / focus workflow
- any custom render-status presentation beyond `RenderingStatus`, `RenderStatusChanged`, and the read-only `InteractionQuality` seam

## Boundary Guidance

- Do not treat `VideraChartView` as a `VideraView` mode.
- Do not push chart-specific semantics back into viewer selection, annotation, or camera contracts.
- Keep input interpretation, tile scheduling, render-host orchestration, and overlay behavior separated.
