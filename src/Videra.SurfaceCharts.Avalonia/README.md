# Videra.SurfaceCharts.Avalonia

`Videra.SurfaceCharts.Avalonia` provides the dedicated Avalonia control surface for the surface-chart module family.

The control layer remains separate from `VideraView` and only depends on the shared surface-chart contracts in `Videra.SurfaceCharts.Core`.

## Current Alpha Scope

`SurfaceChartView` currently provides:

- host-driven surface rendering from an `ISurfaceTileSource`
- host-driven `Viewport` changes in sample space
- overview-first tile scheduling with lazy cache-backed reads
- color-map driven surface rendering
- overlay state for probe/readout behavior

This module is intentionally a thin UI shell. Tile decoding, preprocessing, cache generation, and LOD policy remain outside the control layer.

## Current Limitations

The chart control is still an early alpha surface. Today it does not yet provide a complete end-user interaction stack:

- built-in mouse orbit / pan / zoom is not complete
- axis, tick, label, and legend presentation is not complete
- the demo drives viewport changes through presets rather than a finished interactive camera workflow
- the probe/readout path exists in the control internals, but the public demo does not yet expose a finished interactive hover experience

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
- any higher-level UI that wants zoom, pan, orbit, or axes

## Boundary Guidance

- Do not treat `SurfaceChartView` as a `VideraView` mode.
- Do not push chart-specific semantics back into viewer selection, annotation, or camera contracts.
- Keep input interpretation, tile scheduling, render-scene composition, and overlay behavior separated.
