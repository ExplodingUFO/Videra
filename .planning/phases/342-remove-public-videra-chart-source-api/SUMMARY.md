# Phase 342 Summary: Remove Public VideraChartView Source API

## Bead

- `Videra-85t`
- Status: closed

## Changes

- Removed public `VideraChartView.Source` and `VideraChartView.SourceProperty`.
- Migrated direct control loading in integration tests, demo code, consumer smoke code, and Avalonia README snippets to `VideraChartView.Plot.Add.*`.
- Added test coverage proving the deleted public API does not exist.
- Kept internal `SurfaceChartRuntime.Source` state because it represents chart-local scheduling/rendering truth, not public compatibility.

## Verification

- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Debug -m:1 --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~VideraChartViewStateTests|FullyQualifiedName~VideraChartViewLifecycleTests|FullyQualifiedName~SurfaceChartTileSchedulingTests|FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceAxisOverlayTests|FullyQualifiedName~SurfaceChartInteractionTests|FullyQualifiedName~VideraChartViewWaterfallIntegrationTests"`
  - Passed: 91/91
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests"`
  - Passed: 14/14
- `dotnet build smoke\Videra.SurfaceCharts.ConsumerSmoke\Videra.SurfaceCharts.ConsumerSmoke.csproj -c Debug`
  - Not completed: package restore failed because `Videra.SurfaceCharts.Avalonia` and `Videra.SurfaceCharts.Processing` are package references and no matching package source was available in this local worktree.
- Source scan confirmed no direct public `Source` assignment remains in `src`, `tests`, `samples`, `smoke`, `docs`, or root README paths.

## Handoff

Phase 343 should rename remaining support-summary vocabulary from "Source path/details" to Plot-owned wording and finish demo/smoke/evidence proof cleanup.
