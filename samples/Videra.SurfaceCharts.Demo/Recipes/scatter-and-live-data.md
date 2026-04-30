# Scatter and Live Data

This recipe uses the shipped Videra SurfaceCharts path directly:
`VideraChartView.Plot.Add.Scatter(...)` receives a `ScatterChartData`
dataset, and live updates flow through `DataLogger3D` over
`ScatterColumnarData` / `ScatterColumnarSeries`. There is no wrapper chart
type, no fallback path, and no downshift to a different renderer contract.

## Static Scatter

Use `Plot.Add.Scatter` when the points are already known. Keep metadata bounds
honest because `ScatterChartData` validates point coordinates against the
declared axes.

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

var chart = new VideraChartView();

var metadata = new ScatterChartMetadata(
    new SurfaceAxisDescriptor("X", "mm", minimum: 0, maximum: 100),
    new SurfaceAxisDescriptor("Z", "mm", minimum: 0, maximum: 100),
    new SurfaceValueRange(minimum: -20, maximum: 20));

var series = new ScatterSeries(
    label: "inspection points",
    color: 0xFF2F6FEDu,
    points:
    [
        new ScatterPoint(10, 3, 15),
        new ScatterPoint(28, 8, 32),
        new ScatterPoint(54, -4, 61),
    ]);

var scatterData = new ScatterChartData(metadata, [series]);
chart.Plot.Add.Scatter(scatterData, "inspection scatter");
chart.FitToData();
```

## Live Columnar Scatter

Use `DataLogger3D` for live scatter updates. It owns one mutable columnar
series and preserves the same append/replace/FIFO counters surfaced later by
`ScatterChartData`.

For high-volume live paths, keep `Pickable=false` unless the workflow needs
per-point hit participation. That is the default on `DataLogger3D` and
`ScatterColumnarSeries`.

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

var chart = new VideraChartView();

var live = new DataLogger3D(
    color: 0xFFFFB000u,
    label: "live vibration points",
    isSortedX: true,
    pickable: false,
    fifoCapacity: 2_000);

live.Append(new ScatterColumnarData(
    x: new float[] { 0f, 1f, 2f },
    y: new float[] { 12f, 14f, 13f },
    z: new float[] { 20f, 21f, 22f },
    size: new float[] { 1.2f, 1.2f, 1.2f }));

live.Append(new ScatterColumnarData(
    x: new float[] { 3f, 4f, 5f },
    y: new float[] { 15f, 16f, 15f },
    z: new float[] { 23f, 24f, 25f }));

live.UseLatestWindow(500);

var liveData = new ScatterChartData(
    metadata: new ScatterChartMetadata(
        new SurfaceAxisDescriptor("Time", "s", minimum: 0, maximum: 120),
        new SurfaceAxisDescriptor("Depth", "mm", minimum: 0, maximum: 60),
        new SurfaceValueRange(minimum: 0, maximum: 40)),
    series: [],
    columnarSeries: [live.Series]);

chart.Plot.Add.Scatter(liveData, "live scatter");
chart.FitToData();

var evidence = live.CreateLiveViewEvidence();
var mode = evidence.Mode == DataLogger3DLiveViewMode.LatestWindow
    ? DataLogger3DLiveViewMode.LatestWindow
    : DataLogger3DLiveViewMode.FullData;
var autoscaleDecision = evidence.AutoscaleDecision == DataLogger3DAutoscaleDecision.LatestWindow
    ? DataLogger3DAutoscaleDecision.LatestWindow
    : DataLogger3DAutoscaleDecision.FullData;
```

`CreateLiveViewEvidence()` is the live-data support payload for retained and
visible-window truth:

- `Mode` is `FullData` until `UseLatestWindow(...)` is selected, then
  `LatestWindow`.
- `AppendedPointCount` is the total number of accepted appended points.
- `DroppedPointCount` is the total number of points removed by FIFO trimming.
- `RetainedPointCount` is the current `DataLogger3D.Count`.
- `VisibleStartIndex` and `VisiblePointCount` identify the retained latest
  window.
- `AutoscaleDecision` is `FullData` or `LatestWindow` to match the active live
  view mode.

`ScatterChartData` carries the same dataset-level counters used by rendering
diagnostics and the demo support summary:

- `ColumnarSeriesCount`
- `ColumnarPointCount`
- `PickablePointCount`
- `StreamingAppendBatchCount`
- `StreamingReplaceBatchCount`
- `StreamingDroppedPointCount`
- `LastStreamingDroppedPointCount`
- `ConfiguredFifoCapacity`

## FIFO and Latest-Window Evidence

FIFO capacity controls retained data. If an append or replace exceeds
`fifoCapacity`, the oldest points are trimmed and the dropped counts increase.
`UseLatestWindow(pointCount)` controls the visible evidence window only; it does
not create another data copy and it does not replace FIFO retention.

The demo support summary should report only observed Videra state: active chart
control, runtime and assembly identity, backend/display environment, active
series, scenario id, output capability diagnostics, dataset evidence, retained
columnar points, append/replace counts, FIFO capacity, FIFO drops, pickable
points, and `InteractionQuality`. If a run did not produce a value, leave the
field unavailable or absent according to the existing summary formatter instead
of inventing evidence.

## Scope Boundaries

This recipe is documentation for the current Videra scatter/live path. It does
not introduce adapter layers, public `Source` assignment, PDF/vector export,
backend expansion, hidden fallback/downshift behavior, benchmark claims, or
broad demo workbench behavior.
