# Phase 378 Handoff: Plot Axes Facade and SavePng Convenience

## Scope Completed

- Added `Plot.Axes.X/Y/Z` facade for label, unit, limit, and autoscale convenience.
- Routed X/Z limits through the existing `VideraChartView.ViewState.DataWindow` ownership.
- Routed Y limits/autoscale through the existing `Plot.ColorMap` value range.
- Added `Plot.SavePngAsync(...)` as a caller-path convenience over the existing `CaptureSnapshotAsync(...)` offscreen render path.

## Verification

- `dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~PlotSnapshot"`
- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~VideraChartViewStateTests|FullyQualifiedName~SurfaceAxisOverlayTests"`
- `pwsh -File scripts\Test-SnapshotExportScope.ps1`

All commands passed in `F:\CodeProjects\DotnetCore\Videra-v255-378`.

## Integration Notes

- Bead `Videra-v255.3` remains open by request.
- No Plot3D ownership migration was introduced; the new facade delegates to existing chart view and Plot-owned state.
- Snapshot scope remains PNG-only and chart-local.
