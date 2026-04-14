# Videra.SurfaceCharts.Avalonia

`Videra.SurfaceCharts.Avalonia` provides the dedicated Avalonia control surface for the surface-chart module family.

The control layer remains separate from `VideraView` and only depends on the shared surface-chart contracts in `Videra.SurfaceCharts.Core`.

## Current Scope

`SurfaceChartView` currently provides:

- a chart-local renderer seam through `SurfaceChartRenderHost`; it is not a `VideraView` mode
- a `GPU-first` renderer path with an explicit `software fallback`
- control-visible `RenderingStatus` / `RenderStatusChanged` truth for `ActiveBackend`, `IsFallback`, `FallbackReason`, and `UsesNativeSurface`
- host-driven surface rendering from an `ISurfaceTileSource`
- host-driven `Viewport` changes in sample space
- overview-first tile scheduling with lazy cache-backed reads
- color-map driven surface rendering
- chart-local overlay state for axes, legend, and hover/pinned probe readout

This module is intentionally a thin UI shell. Tile decoding, preprocessing, cache generation, and LOD policy remain outside the control layer.

## Renderer Truth

- `SurfaceChartView` works through a chart-local renderer seam and stays independent from `VideraView`.
- The renderer is `GPU-first`, but `software fallback` remains a shipped path for unsupported or fallback-triggering environments.
- Hosts can inspect `RenderingStatus` and subscribe to `RenderStatusChanged` instead of relying on silent backend switches.
- Linux native GPU hosting currently embeds through X11 handles. On Wayland sessions the chart host uses an `XWayland compatibility` path; compositor-native Wayland surface embedding is not available in this host shell today.

## Current Limitations

The chart control is still an early alpha surface. Today it does not yet provide a complete end-user interaction stack:

- built-in mouse orbit / pan / zoom is not complete
- the demo drives viewport changes through presets rather than a finished interactive camera workflow
- exhaustive native-host validation still depends on the host/platform combination; Linux Wayland validation remains limited to the documented XWayland compatibility path

These limits are intentional documentation, not hidden gaps. The current implementation is primarily the rendering, LOD, and cache boundary.

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
    Viewport = new SurfaceViewport(0d, 0d, matrix.Metadata.Width, matrix.Metadata.Height),
    ColorMap = colorMap
};
```

Hosts currently own:

- source creation
- viewport updates
- color-map selection
- any higher-level UI that wants a finished zoom / pan / orbit workflow or custom render-status presentation

## Boundary Guidance

- Do not treat `SurfaceChartView` as a `VideraView` mode.
- Do not push chart-specific semantics back into viewer selection, annotation, or camera contracts.
- Keep input interpretation, tile scheduling, render-host orchestration, and overlay behavior separated.
