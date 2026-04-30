# Streaming Workspace

This recipe shows how to combine live scatter data with workspace tracking. Multiple charts can have different streaming modes (append with FIFO, replace, static), and the workspace records streaming status for each chart as evidence.

## Live Scatter with FIFO

Create a `DataLogger3D` with a FIFO capacity for bounded memory usage. Append data in batches and optionally trim the visible window.

```csharp
using Videra.SurfaceCharts.Core;

var live = new DataLogger3D(0xFF2F80EDu, label: "Live scatter", fifoCapacity: 10_000);
live.Append(new ScatterColumnarData(xData, yData, zData));
live.UseLatestWindow(2_000);
```

`UseLatestWindow` controls the visible evidence window only. It does not create another data copy and does not replace FIFO retention. The FIFO trims the oldest points when the retained count exceeds `fifoCapacity`.

Build a `ScatterChartData` from the live series and add it to a chart:

```csharp
var chart = new VideraChartView();
var scatterData = new ScatterChartData(
    new ScatterChartMetadata(
        new SurfaceAxisDescriptor("Time", "s", 0, 10),
        new SurfaceAxisDescriptor("Band", "Hz", 0, 10),
        new SurfaceValueRange(0, 1)),
    [],
    [live.Series]);
chart.Plot.Add.Scatter(scatterData, "Live scatter");
```

## Streaming Evidence

`CreateLiveViewEvidence` returns a runtime truth payload for the live data logger. It reports the current mode, retained point count, visible window bounds, and autoscale decision.

```csharp
var evidence = live.CreateLiveViewEvidence();
// evidence.Mode, evidence.RetainedPointCount, evidence.VisiblePointCount
// evidence.AppendedPointCount, evidence.DroppedPointCount
// Evidence is runtime truth, not a benchmark threshold
```

The evidence record is an immutable snapshot. It does not claim benchmark thresholds, renderer-side window crop, or performance guarantees.

## Workspace Streaming Status

Register streaming status with the workspace to include it in workspace evidence. Each chart can have its own streaming configuration.

```csharp
workspace.RegisterStreamingStatus("live-scatter", new SurfaceChartStreamingStatus
{
    UpdateMode = "Append",
    RetainedPointCount = live.Count,
    FifoCapacity = live.FifoCapacity,
    EvidenceOnly = true,
});

var workspaceEvidence = workspace.CreateWorkspaceEvidence();
// Includes Streaming section with per-chart details
```

`SurfaceChartStreamingStatus` is evidence-only — it does not affect chart rendering or data flow. The workspace uses it to produce a complete evidence block that includes streaming diagnostics alongside panel status and link group state.

## Evidence Boundary

Streaming evidence reports runtime truth: point counts, FIFO drops, update mode, and live-view configuration. It does NOT claim benchmark thresholds, renderer-side window crop, or performance guarantees. Use `scripts/Run-Benchmarks.ps1` for numeric measurements.

The `EvidenceOnly` flag on `SurfaceChartStreamingStatus` makes the distinction explicit: streaming status is a diagnostic record, not a control signal. Charts continue to render based on their own data and configuration regardless of what the workspace records.
