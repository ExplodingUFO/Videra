# Videra.SurfaceCharts.Avalonia

`Videra.SurfaceCharts.Avalonia` provides the dedicated Avalonia control surface for the surface-chart module family.

The control layer remains separate from `VideraView` and only depends on the shared surface-chart contracts in `Videra.SurfaceCharts.Core`.

SurfaceChartView now exposes `ViewState` as the primary chart-view contract while `Viewport` remains a compatibility bridge for existing hosts.
SurfaceChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + Left drag` focus zoom on top of the `ViewState` runtime contract.
The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.
The public interaction diagnostics are `InteractionQuality` + `InteractionQualityChanged` with `Interactive` / `Refine`.
Hosts can keep professional axis, grid, and legend behavior chart-local through `OverlayOptions` for formatter, title/unit override, minor ticks, grid plane, and axis-side selection.
The public overlay configuration seam is `SurfaceChartOverlayOptions` through `OverlayOptions`; overlay state types remain internal.

## Current Scope

`SurfaceChartView` currently provides:

- a chart-local renderer seam through `SurfaceChartRenderHost`; it is not a `VideraView` mode
- a `GPU-first` renderer path with an explicit `software fallback`
- control-visible `RenderingStatus` / `RenderStatusChanged` truth for `ActiveBackend`, `IsReady`, `IsFallback`, `FallbackReason`, `UsesNativeSurface`, and `ResidentTileCount`
- host-driven surface rendering from an `ISurfaceTileSource`
- `ViewState` as the primary chart-view contract while `Viewport` remains a compatibility bridge for existing hosts
- host-driven `FitToData()`, `ResetCamera()`, and `ZoomTo(...)` commands
- built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + Left drag` focus zoom
- explicit `InteractionQuality` / `InteractionQualityChanged` diagnostics with `Interactive` and `Refine` interaction-quality states
- public overlay configuration through `SurfaceChartOverlayOptions` / `OverlayOptions`; overlay state types remain internal
- chart-local `OverlayOptions` for formatter, title/unit override, minor ticks, grid plane, and axis-side selection
- overview-first tile scheduling with lazy cache-backed reads
- color-map driven surface rendering
- chart-local axis/legend overlays and hover/pinned probe readout, including `Shift + LeftClick` pinning
- `SurfaceChartView` owns chart-local built-in gestures, tile scheduling/cache, overlay presentation, native-host/render-host orchestration, and `RenderingStatus` projection

This module is intentionally a thin UI shell. Tile decoding, preprocessing, cache generation, and LOD policy remain outside the control layer.

## Renderer Truth

- `SurfaceChartView` works through a chart-local renderer seam and stays independent from `VideraView`.
- The renderer is `GPU-first`, but `software fallback` remains a shipped path for unsupported or fallback-triggering environments.
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
    <charts:SurfaceChartView x:Name="ChartView" />
</Window>
```

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

var chartView = new SurfaceChartView
{
    Source = new SurfacePyramidBuilder(32, 32).Build(matrix),
    ViewState = SurfaceViewState.CreateDefault(
        matrix.Metadata,
        new SurfaceDataWindow(0d, 0d, matrix.Metadata.Width, matrix.Metadata.Height)),
    ColorMap = colorMap,
    OverlayOptions = new SurfaceChartOverlayOptions
    {
        ShowMinorTicks = true,
        GridPlane = SurfaceChartGridPlane.XZ,
        LabelFormatter = static (_, value) => value.ToString("0.##")
    }
};

chartView.ZoomTo(new SurfaceDataWindow(64d, 32d, 128d, 96d));
chartView.ResetCamera();
```

Hosts currently own:

- `ISurfaceTileSource` creation
- persisted `ViewState` and compatibility `Viewport` updates when older hosts still depend on them
- color-map selection
- any chart-local product UI layered on top of the built-in orbit / pan / dolly / focus workflow
- any custom render-status presentation beyond `RenderingStatus`, `RenderStatusChanged`, and the read-only `InteractionQuality` seam

## Boundary Guidance

- Do not treat `SurfaceChartView` as a `VideraView` mode.
- Do not push chart-specific semantics back into viewer selection, annotation, or camera contracts.
- Keep input interpretation, tile scheduling, render-host orchestration, and overlay behavior separated.
