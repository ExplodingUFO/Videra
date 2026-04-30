# Multi-Chart Analysis Workspace

This recipe shows how to coordinate multiple `VideraChartView` instances through a host-owned `SurfaceChartWorkspace`. The workspace tracks registered charts, their panel metadata, and the active selection. It does not own chart lifecycle — charts are created and destroyed by the host.

## Creating a Workspace

The workspace is a lightweight coordination layer. It does not create, render, or dispose chart views. The host is responsible for chart lifetime.

```csharp
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;

var workspace = new SurfaceChartWorkspace();
```

## Registering Charts

Register each chart with a `SurfaceChartPanelInfo` that describes its identity, label, and chart family. The first registered chart automatically becomes the active chart.

```csharp
var chartA = new VideraChartView();
chartA.Plot.Add.Surface(new double[,]
{
    { 0.0, 0.4, 0.8 },
    { 0.2, 0.7, 1.0 },
    { 0.1, 0.5, 0.9 },
}, "Surface A");

var chartB = new VideraChartView();
chartB.Plot.Add.Contour(contourField, explicitLevels: [0.25f, 0.5f, 0.75f], name: "Contour B");

workspace.Register(chartA, new SurfaceChartPanelInfo(
    ChartId: "surface-a",
    Label: "Surface A",
    ChartKind: Plot3DSeriesKind.Surface));

workspace.Register(chartB, new SurfaceChartPanelInfo(
    ChartId: "contour-b",
    Label: "Contour B",
    ChartKind: Plot3DSeriesKind.Contour));
```

Each `ChartId` must be unique within the workspace. Registering a duplicate throws `InvalidOperationException`.

## Setting Active Chart

Use `SetActiveChart` to change which panel is focused. The active chart id is returned by `GetActiveChartId`.

```csharp
workspace.SetActiveChart("contour-b");
var activeId = workspace.GetActiveChartId();
// activeId == "contour-b"
```

## Capturing Evidence

The workspace produces two evidence artifacts: a status snapshot and a bounded text evidence block.

```csharp
var status = workspace.CaptureWorkspaceStatus();
// status.PanelCount, status.ActiveChartId, status.AllReady, status.Panels[]

var evidence = workspace.CreateWorkspaceEvidence();
// Copy evidence to clipboard for diagnostics
```

`CaptureWorkspaceStatus` returns per-chart rendering readiness, series count, and point count. `CreateWorkspaceEvidence` returns a bounded text block suitable for copy-paste into support reports.

## Cleanup

Disposing the workspace clears its internal references. It does not dispose the registered chart views — the host owns their lifecycle.

```csharp
workspace.Dispose();
```

The workspace is deliberately not a control or visual element. It is a host-owned coordination object that tracks metadata and produces evidence. Charts remain independent `VideraChartView` instances managed by the host layout.
