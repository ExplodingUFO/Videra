# Phase 340 Summary: Plot-Owned Surface and Waterfall Activation

**Bead:** Videra-1ug  
**Status:** Complete  
**Date:** 2026-04-28  

## Result

`VideraChartView.Plot.Add.Surface(...)` and `VideraChartView.Plot.Add.Waterfall(...)` now activate chart runtime source state without requiring public `VideraChartView.Source` assignment.

The implementation keeps the existing runtime path:

- `Plot3D.ActiveSurfaceSeries` selects the latest surface or waterfall series.
- `VideraChartView.OnPlotChanged()` compares the selected source with `_runtime.Source`.
- source replacement still routes through `OnSourceChanged(...)`.
- `OnSourceChanged(...)` still resets overlay/projection/failure state and calls `_runtime.UpdateSource(...)`.
- `Plot.Clear()` clears active runtime source state.

The public `Source` property still exists until Phase 342, but the new tests prove Plot-owned activation works while `view.Source` remains unset.

## Files Changed

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`

## Verification

Passed:

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Debug --no-restore -m:1 --filter "FullyQualifiedName~VideraChartViewPlotApiTests"
```

Passed:

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Debug --no-restore -m:1 --filter "FullyQualifiedName~VideraChartViewStateTests|FullyQualifiedName~SurfaceChartInteractionTests|FullyQualifiedName~VideraChartViewWaterfallIntegrationTests|FullyQualifiedName~VideraChartViewPlotApiTests"
```

## Handoff

Phase 341 should add the equivalent Plot-owned scatter state/status path. Phase 342 can delete public `Source` / `SourceProperty` after both activation families are complete.
