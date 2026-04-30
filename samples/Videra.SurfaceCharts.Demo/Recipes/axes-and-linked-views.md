# Axes And Linked Views Recipe

This recipe shows the native Videra axes, overlay, and linked-view path for two `VideraChartView` instances. The link synchronizes `ViewState`; it does not merge plot models or share data ownership.

## Axis Labels, Units, And Limits

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;

var chart = new VideraChartView();

chart.Plot.Add.Waterfall(new double[,]
{
    { 0.10, 0.35, 0.62, 0.50 },
    { 0.18, 0.48, 0.88, 0.72 },
    { 0.25, 0.58, 1.00, 0.83 },
    { 0.20, 0.44, 0.78, 0.66 },
}, "Waterfall response");

chart.Plot.Axes.X.Label = "Sweep";
chart.Plot.Axes.X.Unit = "index";
chart.Plot.Axes.Y.Label = "Amplitude";
chart.Plot.Axes.Y.Unit = "g";
chart.Plot.Axes.Z.Label = "Band";
chart.Plot.Axes.Z.Unit = "Hz";

chart.Plot.Axes.X.SetLimits(0, 3);
chart.Plot.Axes.Y.SetBounds(0, 1.2);
chart.Plot.Axes.Z.AutoScale();
chart.FitToData();
```

Use `SetLimits(...)` to move the current visible data window. Use `SetBounds(...)` to constrain later limit and autoscale calls. Use `ClearBounds()` when a host preference should stop constraining that axis.

```csharp
chart.Plot.Axes.X.ClearBounds();
chart.Plot.Axes.AutoScale();
```

## Overlay Options

`Plot.OverlayOptions` is the public chart-local formatting entrypoint for labels, grid, legend, minor ticks, crosshair, and probe text.

```csharp
chart.Plot.OverlayOptions = new SurfaceChartOverlayOptions
{
    ShowMinorTicks = true,
    MinorTickDivisions = 4,
    GridPlane = SurfaceChartGridPlane.XZ,
    AxisSideMode = SurfaceChartAxisSideMode.Auto,
    TickLabelFormat = SurfaceChartNumericLabelFormat.Fixed,
    TickLabelPrecision = 2,
    LegendLabelFormat = SurfaceChartNumericLabelFormat.Engineering,
    LegendLabelPrecision = 2,
    XAxisFormatter = value => $"{value:0.0} s",
    YAxisFormatter = value => $"{value:0.00} g",
    ZAxisFormatter = value => $"{value:0} Hz",
    LegendTitleOverride = "Amplitude",
    ShowCrosshair = true,
    LegendPosition = SurfaceChartLegendPosition.TopRight,
};
```

Prefer axis `Label` and `Unit` for semantic titles. Prefer `Plot.OverlayOptions` when the requirement is presentation: precision, formatter, grid plane, visible side, legend position, minor ticks, or crosshair behavior.

## Linked Views

`LinkViewWith` returns an `IDisposable` lifetime. Keep that lifetime for as long as the views should mirror each other's `ViewState`; dispose it when the user closes the comparison panel or disables synchronization.

```csharp
var left = new VideraChartView();
var right = new VideraChartView();

left.Plot.Add.Waterfall(CreateBaselineMatrix(), "Baseline");
right.Plot.Add.Waterfall(CreateCurrentMatrix(), "Current");

left.Plot.Axes.X.Label = "Sweep";
left.Plot.Axes.Y.Label = "Amplitude";
left.Plot.Axes.Z.Label = "Band";

right.Plot.Axes.X.Label = "Sweep";
right.Plot.Axes.Y.Label = "Amplitude";
right.Plot.Axes.Z.Label = "Band";

using var linkedView = left.LinkViewWith(right);

left.FitToData();
var synchronized = right.ViewState;

right.Plot.Axes.Y.SetBounds(0, 1.2);
right.FitToData();
```

The first `LinkViewWith` call copies the left `ViewState` into the right chart, then listens for `ViewState` changes in either direction. Calling `FitToData()` on either chart updates that chart's `ViewState`, so the peer receives the same camera and data window while the link is active.

## Disposable Lifetime In A View Model

```csharp
private IDisposable? _linkedView;

public void EnableLinkedViews(VideraChartView left, VideraChartView right)
{
    _linkedView?.Dispose();
    _linkedView = left.LinkViewWith(right);
}

public void DisableLinkedViews()
{
    _linkedView?.Dispose();
    _linkedView = null;
}
```

The lifetime is deliberately explicit. It prevents stale event subscriptions after comparison views are removed, and it makes link ownership visible in host code.

## Fit And Restore

Use `FitToData()` when the operator asks to see the full active dataset. Use `ViewState` when restoring a known camera or preserving a linked comparison pose.

```csharp
var beforeRefresh = left.ViewState;

left.Plot.Clear();
left.Plot.Add.Waterfall(CreateRefreshedMatrix(), "Baseline refresh");
left.ViewState = beforeRefresh;

right.FitToData();
```

For linked comparisons, make this choice explicit in the host UI: restore a saved `ViewState` to keep the comparison pose, or call `FitToData()` to reframe both charts through the active link.
