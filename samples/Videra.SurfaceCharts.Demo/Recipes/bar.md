# Bar Recipe

This recipe shows the bounded Videra-native grouped bar path used by
`Videra.SurfaceCharts.Demo`. It uses the shipped `VideraChartView` control and
the Plot API entrypoint `Plot.Add.Bar`; it is not a compatibility wrapper for an
old chart control or a ScottPlot adapter.

## Setup

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
```

Create one chart control and one immutable `BarChartData` value. Each
`BarSeries` must have the same category count. The demo proof uses three named
series and five categories so the renderer status can report deterministic
support evidence.

```csharp
var chart = new VideraChartView();

var data = new BarChartData(
[
    new BarSeries([12.0, 19.0, 3.0, 5.0, 8.0], 0xFF38BDF8u, "Series A"),
    new BarSeries([7.0, 11.0, 15.0, 8.0, 13.0], 0xFFF97316u, "Series B"),
    new BarSeries([5.0, 9.0, 12.0, 18.0, 6.0], 0xFF2DD4BFu, "Series C"),
], BarChartLayout.Grouped);

chart.Plot.Clear();
chart.Plot.Add.Bar(data, "Grouped bars");
chart.FitToData();
```

## Demo Proof

In the sample app, select `Try next: Bar chart proof`. The visible proof is
authored through `VideraChartView.Plot.Add.Bar` and keeps the chart path inside
the same SurfaceCharts demo shell as the other cookbook recipes.

The support summary for this proof includes:

- `EvidenceKind: SurfaceChartsDatasetProof`
- `Plot path: Try next: Bar chart proof`
- `Chart contract: VideraChartView exposes Plot.Add.Bar on this proof path.`
- `BarRenderingStatus` values through the visible rendering-path panel:
  `Series`, `Categories`, `Bars`, and `Layout`
- `OutputCapabilityDiagnostics` from `Plot3DOutputCapabilityDiagnostic`

These values are support evidence only. They are not stable benchmark
guarantees and should not be presented as performance measurements.

## Boundaries

Use `Plot.Add.Bar` for Bar data loading. Do not add `SurfaceChartView`,
`WaterfallChartView`, `ScatterChartView`, a public `VideraChartView.Source`
loader, a ScottPlot compatibility layer, hidden data fallback, or a generic
workbench to make this recipe work.
