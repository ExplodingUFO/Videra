# Phase 341 Summary: Plot-Owned Scatter Activation

**Bead:** Videra-mov  
**Status:** Complete  
**Date:** 2026-04-28  

## Result

`VideraChartView.Plot.Add.Scatter(...)` now drives a Plot-owned scatter status path through `VideraChartView.ScatterRenderingStatus`.

The status is derived from the active Plot scatter series and includes:

- readiness and software backend kind
- interaction quality and active gesture state
- point-series count
- columnar series count
- retained columnar point count
- pickable point count
- append/replace batch counters
- FIFO capacity and dropped-point counters
- view size and camera summary fields

The active Plot semantics are now last-added series:

- active surface/waterfall series drives the surface runtime source path
- active scatter series drives scatter status/evidence
- adding a surface after scatter clears active scatter status

Demo scatter diagnostics now read `VideraChartView.ScatterRenderingStatus` instead of using only local `_activeScatterData`.

## Files Changed

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Properties.cs`
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`

## Verification

Passed:

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Debug --no-restore -m:1 --filter "FullyQualifiedName~VideraChartViewPlotApiTests"
```

Passed:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests"
```

## Handoff

Phase 342 can delete public `VideraChartView.Source` and `SourceProperty` because both surface/waterfall and scatter Plot-owned activation paths now exist.
