---
phase: 373
plan: 373-PLAN
type: auto
autonomous: true
wave: 1
depends_on: []
requirements: [PROBE-01, PROBE-02, PROBE-03, PROBE-04]
---

# Phase 373 Plan: Series Probe Strategies

## Objective

Implement mouse-driven probe resolution for scatter, bar, and contour series via an extensible `ISeriesProbeStrategy` interface, filling the gap where only surface/waterfall probes existed.

## Context

- @.planning/phases/373-series-probe-strategies/CONTEXT.md
- @src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs
- @src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeRequest.cs
- @src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs
- @src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs
- @src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs
- @src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs

## Tasks

### Task 1: Create ISeriesProbeStrategy Interface
**Type:** auto
**Description:** Define the `ISeriesProbeStrategy` interface in `Videra.SurfaceCharts.Core/Picking/` with a single `TryResolve` method that takes chart-space coordinates and returns a `SurfaceProbeInfo?`.

**Implementation:**
```csharp
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probe information for a specific series kind at a given chart-space position.
/// </summary>
public interface ISeriesProbeStrategy
{
    /// <summary>
    /// Attempts to resolve a probe at the specified chart-space coordinates.
    /// </summary>
    /// <param name="chartX">The horizontal chart-space coordinate.</param>
    /// <param name="chartZ">The depth chart-space coordinate.</param>
    /// <param name="metadata">The surface metadata for axis mapping.</param>
    /// <returns>A <see cref="SurfaceProbeInfo"/> if a probe was resolved; otherwise, <c>null</c>.</returns>
    SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata);
}
```

**Files:** `src/Videra.SurfaceCharts.Core/Picking/ISeriesProbeStrategy.cs` (new)
**Verification:** Interface compiles, has single method returning `SurfaceProbeInfo?`
**Done criteria:** Interface defined with XML docs

---

### Task 2: Implement ScatterProbeStrategy
**Type:** auto
**Description:** Implement brute-force nearest-point lookup for scatter series. Takes chart-space XZ coordinates, finds the nearest `ScatterPoint` by Euclidean distance in the XZ plane, returns `SurfaceProbeInfo` with the point's coordinates and value.

**Implementation:**
```csharp
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for scatter series by finding the nearest data point.
/// </summary>
public sealed class ScatterProbeStrategy : ISeriesProbeStrategy
{
    private readonly ScatterChartData _data;

    public ScatterProbeStrategy(ScatterChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _data = data;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        ScatterPoint? nearest = null;
        var minDistSq = double.MaxValue;

        foreach (var series in _data.Series)
        {
            foreach (var point in series.Points)
            {
                var dx = point.Horizontal - chartX;
                var dz = point.Depth - chartZ;
                var distSq = (dx * dx) + (dz * dz);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    nearest = point;
                }
            }
        }

        // Also check columnar series
        foreach (var series in _data.ColumnarSeries)
        {
            var xs = series.X.Span;
            var ys = series.Y.Span;
            var zs = series.Z.Span;
            for (var i = 0; i < series.Count; i++)
            {
                if (float.IsNaN(xs[i]) || float.IsNaN(ys[i]) || float.IsNaN(zs[i]))
                    continue;

                var dx = xs[i] - chartX;
                var dz = zs[i] - chartZ;
                var distSq = (dx * dx) + (dz * dz);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    nearest = new ScatterPoint(xs[i], ys[i], zs[i]);
                }
            }
        }

        if (nearest is not ScatterPoint hit)
            return null;

        return new SurfaceProbeInfo(
            sampleX: hit.Horizontal,
            sampleY: hit.Depth,
            axisX: hit.Horizontal,
            axisY: hit.Depth,
            value: hit.Value,
            isApproximate: false);
    }
}
```

**Files:** `src/Videra.SurfaceCharts.Core/Picking/ScatterProbeStrategy.cs` (new)
**Verification:** Strategy compiles, handles empty series, handles columnar series
**Done criteria:** ScatterProbeStrategy implemented with brute-force nearest-point

---

### Task 3: Implement BarProbeStrategy
**Type:** auto
**Description:** Implement bar hit-test for bar chart series. Takes chart-space XZ coordinates, checks which bar contains the position using AABB containment, returns `SurfaceProbeInfo` with bar value and category.

**Implementation:**
```csharp
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for bar chart series by hit-testing bar bounding boxes.
/// </summary>
public sealed class BarProbeStrategy : ISeriesProbeStrategy
{
    private readonly BarRenderScene _scene;

    public BarProbeStrategy(BarRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        BarRenderBar? hitBar = null;
        var minDistSq = double.MaxValue;

        foreach (var bar in _scene.Bars)
        {
            var halfWidth = bar.Size.X / 2f;
            var halfDepth = bar.Size.Z / 2f;

            // Check if chartX, chartZ is within the bar's XZ footprint
            var dx = chartX - bar.Position.X;
            var dz = chartZ - bar.Position.Z;

            if (Math.Abs(dx) <= halfWidth && Math.Abs(dz) <= halfDepth)
            {
                // Inside bar footprint — prefer the bar with smallest distance to center
                var distSq = (dx * dx) + (dz * dz);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    hitBar = bar;
                }
            }
        }

        if (hitBar is not BarRenderBar hit)
            return null;

        // Bar value is the height (Y size), bar position Y is the base
        var value = hit.Position.Y + hit.Size.Y;

        return new SurfaceProbeInfo(
            sampleX: hit.Position.X,
            sampleY: hit.Position.Z,
            axisX: hit.Position.X,
            axisY: hit.Position.Z,
            value: value,
            isApproximate: false);
    }
}
```

**Files:** `src/Videra.SurfaceCharts.Core/Picking/BarProbeStrategy.cs` (new)
**Verification:** Strategy compiles, handles empty bars, correct AABB check
**Done criteria:** BarProbeStrategy implemented with bar hit-test

---

### Task 4: Implement ContourProbeStrategy
**Type:** auto
**Description:** Implement contour line hit-test for contour series. Takes chart-space XZ coordinates, finds the nearest contour segment by point-to-segment distance, returns `SurfaceProbeInfo` with the contour line's iso-value.

**Implementation:**
```csharp
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for contour series by finding the nearest contour line segment.
/// </summary>
public sealed class ContourProbeStrategy : ISeriesProbeStrategy
{
    private readonly ContourRenderScene _scene;
    private readonly double _snapRadius;

    public ContourProbeStrategy(ContourRenderScene scene, double snapRadius = 0.05)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
        _snapRadius = snapRadius;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        ContourLine? nearestLine = null;
        ContourSegment nearestSegment = default;
        var minDist = double.MaxValue;

        foreach (var line in _scene.Lines)
        {
            foreach (var segment in line.Segments)
            {
                var dist = PointToSegmentDistance(
                    chartX, chartZ,
                    segment.Start.X, segment.Start.Z,
                    segment.End.X, segment.End.Z);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearestLine = line;
                    nearestSegment = segment;
                }
            }
        }

        if (nearestLine is null || minDist > _snapRadius)
            return null;

        // Project cursor onto the nearest segment to get the sample position
        var t = ProjectOntoSegment(
            chartX, chartZ,
            nearestSegment.Start.X, nearestSegment.Start.Z,
            nearestSegment.End.X, nearestSegment.End.Z);
        var sampleX = nearestSegment.Start.X + (t * (nearestSegment.End.X - nearestSegment.Start.X));
        var sampleZ = nearestSegment.Start.Z + (t * (nearestSegment.End.Z - nearestSegment.Start.Z));

        return new SurfaceProbeInfo(
            sampleX: sampleX,
            sampleY: sampleZ,
            axisX: sampleX,
            axisY: sampleZ,
            value: nearestLine.IsoValue,
            isApproximate: false);
    }

    private static double PointToSegmentDistance(
        double px, double pz,
        double ax, double az,
        double bx, double bz)
    {
        var abx = bx - ax;
        var abz = bz - az;
        var apx = px - ax;
        var apz = pz - az;

        var abLenSq = (abx * abx) + (abz * abz);
        if (abLenSq < 1e-12)
        {
            // Degenerate segment — return point-to-point distance
            var ddx = px - ax;
            var ddz = pz - az;
            return Math.Sqrt((ddx * ddx) + (ddz * ddz));
        }

        var t = Math.Clamp(((apx * abx) + (apz * abz)) / abLenSq, 0d, 1d);
        var projX = ax + (t * abx);
        var projZ = az + (t * abz);
        var dx = px - projX;
        var dz = pz - projZ;
        return Math.Sqrt((dx * dx) + (dz * dz));
    }

    private static double ProjectOntoSegment(
        double px, double pz,
        double ax, double az,
        double bx, double bz)
    {
        var abx = bx - ax;
        var abz = bz - az;
        var apx = px - ax;
        var apz = pz - az;

        var abLenSq = (abx * abx) + (abz * abz);
        if (abLenSq < 1e-12)
            return 0d;

        return Math.Clamp(((apx * abx) + (apz * abz)) / abLenSq, 0d, 1d);
    }
}
```

**Files:** `src/Videra.SurfaceCharts.Core/Picking/ContourProbeStrategy.cs` (new)
**Verification:** Strategy compiles, handles empty contours, correct distance calculation
**Done criteria:** ContourProbeStrategy implemented with segment distance check

---

### Task 5: Create SeriesProbeStrategyDispatcher
**Type:** auto
**Description:** Create a dispatcher that maps `Plot3DSeriesKind` to the correct `ISeriesProbeStrategy` and provides a unified `TryResolve` method.

**Implementation:**
```csharp
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Dispatches probe resolution to the appropriate strategy based on series kind.
/// </summary>
public sealed class SeriesProbeStrategyDispatcher
{
    private readonly Dictionary<Plot3DSeriesKind, ISeriesProbeStrategy> _strategies;

    public SeriesProbeStrategyDispatcher(IReadOnlyDictionary<Plot3DSeriesKind, ISeriesProbeStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        _strategies = new Dictionary<Plot3DSeriesKind, ISeriesProbeStrategy>(strategies);
    }

    /// <summary>
    /// Attempts to resolve a probe for the specified series kind.
    /// </summary>
    public SurfaceProbeInfo? TryResolve(Plot3DSeriesKind kind, double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (_strategies.TryGetValue(kind, out var strategy))
        {
            return strategy.TryResolve(chartX, chartZ, metadata);
        }

        return null;
    }
}
```

**Files:** `src/Videra.SurfaceCharts.Core/Picking/SeriesProbeStrategyDispatcher.cs` (new)
**Verification:** Dispatcher compiles, handles missing strategies gracefully
**Done criteria:** Dispatcher maps series kinds to strategies

---

### Task 6: Integrate Strategies into SurfaceProbeService
**Type:** auto
**Description:** Add a new `ResolveFromScreenPosition` overload to `SurfaceProbeService` that accepts a `SeriesProbeStrategyDispatcher` and dispatches to the correct strategy for non-surface series.

**Implementation:**
Add to `SurfaceProbeService.cs`:
```csharp
/// <summary>
/// Resolves a probe for non-surface series using the strategy dispatcher.
/// </summary>
public static SurfaceProbeInfo? ResolveFromScreenPosition(
    SurfaceMetadata metadata,
    SurfaceViewport viewport,
    Size viewSize,
    SeriesProbeStrategyDispatcher dispatcher,
    Plot3DSeriesKind seriesKind,
    Point probeScreenPosition)
{
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentNullException.ThrowIfNull(dispatcher);

    if (viewSize.Width <= 0d || viewSize.Height <= 0d)
        return null;

    var clampedViewport = viewport.ClampTo(metadata);
    var normalizedX = Math.Clamp(probeScreenPosition.X / viewSize.Width, 0d, 1d);
    var normalizedY = Math.Clamp(probeScreenPosition.Y / viewSize.Height, 0d, 1d);

    var chartX = clampedViewport.StartX + (normalizedX * clampedViewport.Width);
    var chartZ = clampedViewport.StartY + (normalizedY * clampedViewport.Height);

    return dispatcher.TryResolve(seriesKind, chartX, chartZ, metadata);
}
```

**Files:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs` (modify)
**Verification:** New overload compiles, integrates with existing service
**Done criteria:** SurfaceProbeService has strategy-based resolution path

---

### Task 7: Wire Strategies into SurfaceProbeOverlayPresenter
**Type:** auto
**Description:** Modify `SurfaceProbeOverlayPresenter.CreateState` to accept an optional `SeriesProbeStrategyDispatcher` and use it for non-surface series (Scatter, Bar, Contour).

**Implementation:**
Add new `CreateState` overload that handles non-surface series:
```csharp
public static SurfaceProbeOverlayState CreateState(
    Plot3DSeries? activeSeries,
    SurfaceViewport viewport,
    Size viewSize,
    SurfaceMetadata metadata,
    SeriesProbeStrategyDispatcher? strategyDispatcher,
    Point? probeScreenPosition,
    IReadOnlyList<SurfaceProbeRequest>? pinnedProbeRequests = null,
    SurfaceChartOverlayOptions? overlayOptions = null)
{
    overlayOptions ??= SurfaceChartOverlayOptions.Default;

    if (activeSeries is null || strategyDispatcher is null)
    {
        return new SurfaceProbeOverlayState(
            hasNoData: activeSeries is null,
            noDataText: activeSeries is null ? "No data" : null,
            hoveredProbeScreenPosition: null,
            hoveredProbe: null,
            pinnedProbes: [],
            overlayOptions: overlayOptions);
    }

    var hoveredProbe = probeScreenPosition is Point hoveredPos
        ? SurfaceProbeService.ResolveFromScreenPosition(
            metadata, viewport, viewSize, strategyDispatcher, activeSeries.Kind, hoveredPos)
        : null;

    return new SurfaceProbeOverlayState(
        hasNoData: false,
        noDataText: null,
        hoveredProbeScreenPosition: hoveredProbe is null ? null : probeScreenPosition,
        hoveredProbe: hoveredProbe,
        pinnedProbes: [],
        overlayOptions: overlayOptions);
}
```

**Files:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs` (modify)
**Verification:** New overload compiles, integrates with existing presenter
**Done criteria:** Presenter dispatches to strategies for non-surface series

---

### Task 8: Write Unit Tests
**Type:** auto
**Description:** Create comprehensive unit tests for all three probe strategies and the dispatcher.

**Test cases:**
1. `ScatterProbeStrategy` — nearest point found, empty series returns null, columnar series included
2. `BarProbeStrategy` — bar hit detected, outside bar returns null, overlapping bars handled
3. `ContourProbeStrategy` — nearest segment found, snap radius respected, iso-value correct
4. `SeriesProbeStrategyDispatcher` — dispatches to correct strategy, missing strategy returns null

**Files:** `tests/Videra.SurfaceCharts.Core.Tests/Picking/SeriesProbeStrategyTests.cs` (new)
**Verification:** All tests pass
**Done criteria:** Tests cover all strategies with positive and negative cases

---

## Verification / Success Criteria

1. `ISeriesProbeStrategy` interface defined in Core with single `TryResolve` method
2. `ScatterProbeStrategy` finds nearest point via brute-force XZ distance
3. `BarProbeStrategy` hit-tests bar AABB footprints
4. `ContourProbeStrategy` finds nearest contour segment within snap radius
5. `SeriesProbeStrategyDispatcher` maps series kinds to strategies
6. `SurfaceProbeService` has strategy-based resolution path for non-surface series
7. `SurfaceProbeOverlayPresenter` dispatches to strategies for Scatter/Bar/Contour
8. All unit tests pass
9. No existing tests broken

## Output Spec

- New files: 5 source files + 1 test file
- Modified files: 2 (SurfaceProbeService, SurfaceProbeOverlayPresenter)
- Tests: Unit tests for all strategies
- Commits: 1 per task (8 total)
